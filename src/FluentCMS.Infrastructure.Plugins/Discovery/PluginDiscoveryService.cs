using System.Reflection;
using System.Runtime.Loader;
using FluentCMS.Infrastructure.Core.Contracts;
using FluentCMS.Infrastructure.Plugins.Options;
using FluentCMS.Infrastructure.Storage.Data;
using FluentCMS.Infrastructure.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentCMS.Infrastructure.Plugins.Discovery;

public class PluginDiscoveryService : IPluginDiscoveryService
{
    private readonly PluginDbContext _dbContext;
    private readonly ILogger<PluginDiscoveryService> _logger;
    private readonly string _pluginsDirectory;
    private readonly bool _autoEnableNewPlugins;

    public PluginDiscoveryService(
        PluginDbContext dbContext,
        ILogger<PluginDiscoveryService> logger,
        IOptions<PluginOptions> options)
    {
        _dbContext = dbContext;
        _logger = logger;
        _pluginsDirectory = options.Value.PluginsDirectory;
        _autoEnableNewPlugins = options.Value.AutoEnableNewPlugins;
        
        // Ensure plugins directory exists
        if (!Directory.Exists(_pluginsDirectory))
        {
            _logger.LogInformation("Creating plugins directory: {PluginsDirectory}", _pluginsDirectory);
            Directory.CreateDirectory(_pluginsDirectory);
        }
    }

    public async Task<IEnumerable<PluginMetadata>> DiscoverPlugins(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Discovering plugins in directory: {PluginsDirectory}", _pluginsDirectory);
        
        var pluginFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.AllDirectories);
        _logger.LogDebug("Found {Count} DLL files in plugins directory", pluginFiles.Length);
        
        var discoveredPlugins = new List<PluginMetadata>();
        
        foreach (var pluginFile in pluginFiles)
        {
            try
            {
                var pluginMetadata = await ScanPluginAssembly(pluginFile, cancellationToken);
                if (pluginMetadata != null)
                {
                    discoveredPlugins.Add(pluginMetadata);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning plugin assembly: {PluginFile}", pluginFile);
            }
        }
        
        _logger.LogInformation("Discovered {Count} valid plugins", discoveredPlugins.Count);
        
        // Sync with database
        await SyncPluginsWithDatabase(discoveredPlugins, cancellationToken);
        
        return await _dbContext.Plugins.ToListAsync(cancellationToken);
    }

    public async Task<PluginMetadata> RegisterPlugin(string assemblyPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering plugin from assembly: {AssemblyPath}", assemblyPath);
        
        if (!File.Exists(assemblyPath))
        {
            _logger.LogError("Plugin assembly file not found: {AssemblyPath}", assemblyPath);
            return null;
        }
        
        var pluginMetadata = await ScanPluginAssembly(assemblyPath, cancellationToken);
        if (pluginMetadata == null)
        {
            _logger.LogError("Assembly does not contain a valid plugin: {AssemblyPath}", assemblyPath);
            return null;
        }
        
        var existingPlugin = await _dbContext.Plugins
            .FirstOrDefaultAsync(p => p.Id == pluginMetadata.Id, cancellationToken);
            
        if (existingPlugin != null)
        {
            // Update existing metadata
            existingPlugin.Name = pluginMetadata.Name;
            existingPlugin.Version = pluginMetadata.Version;
            existingPlugin.AssemblyPath = pluginMetadata.AssemblyPath;
            
            _logger.LogInformation("Updated existing plugin: {PluginId}", pluginMetadata.Id);
        }
        else
        {
            // Add new plugin metadata
            pluginMetadata.IsEnabled = _autoEnableNewPlugins;
            pluginMetadata.InstalledDate = DateTime.UtcNow;
            
            await _dbContext.Plugins.AddAsync(pluginMetadata, cancellationToken);
            
            _logger.LogInformation("Added new plugin: {PluginId}, AutoEnabled: {AutoEnabled}", 
                pluginMetadata.Id, _autoEnableNewPlugins);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return pluginMetadata;
    }

    public async Task<bool> EnablePlugin(string pluginId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enabling plugin: {PluginId}", pluginId);
        
        var plugin = await _dbContext.Plugins
            .FirstOrDefaultAsync(p => p.Id == pluginId, cancellationToken);
            
        if (plugin == null)
        {
            _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
            return false;
        }
        
        plugin.IsEnabled = true;
        plugin.LastEnabledDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Plugin enabled: {PluginId}", pluginId);
        return true;
    }

    public async Task<bool> DisablePlugin(string pluginId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disabling plugin: {PluginId}", pluginId);
        
        var plugin = await _dbContext.Plugins
            .FirstOrDefaultAsync(p => p.Id == pluginId, cancellationToken);
            
        if (plugin == null)
        {
            _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
            return false;
        }
        
        plugin.IsEnabled = false;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Plugin disabled: {PluginId}", pluginId);
        return true;
    }

    // Helper method to scan an assembly for plugin types
    private async Task<PluginMetadata> ScanPluginAssembly(string assemblyPath, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Scanning assembly: {AssemblyPath}", assemblyPath);
        
        // Load assembly in a separate AssemblyLoadContext to avoid locking the file
        var context = new CustomAssemblyLoadContext();
        
        try
        {
            using var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read);
            var assembly = context.LoadFromStream(stream);
            
            // Look for types implementing IPlugin
            var pluginType = assembly.GetTypes()
                .FirstOrDefault(t => 
                    !t.IsAbstract && 
                    typeof(IPlugin).IsAssignableFrom(t));
            
            if (pluginType == null)
            {
                _logger.LogDebug("No plugin type found in assembly: {AssemblyPath}", assemblyPath);
                return null;
            }
            
            // Create plugin instance to get metadata
            var plugin = (IPlugin)Activator.CreateInstance(pluginType);
            
            var metadata = new PluginMetadata
            {
                Id = plugin.Id,
                Name = plugin.Name,
                Version = plugin.Version,
                AssemblyPath = assemblyPath
            };
            
            _logger.LogDebug("Found plugin: {PluginId}, {PluginName}, {PluginVersion}", 
                metadata.Id, metadata.Name, metadata.Version);
            
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning assembly: {AssemblyPath}", assemblyPath);
            return null;
        }
        finally
        {
            // Unload context to release assembly
            context.Unload();
        }
    }
    
    // Helper method to synchronize discovered plugins with database
    private async Task SyncPluginsWithDatabase(List<PluginMetadata> discoveredPlugins, CancellationToken cancellationToken)
    {
        var dbPlugins = await _dbContext.Plugins.ToListAsync(cancellationToken);
        
        // Add new plugins
        foreach (var plugin in discoveredPlugins)
        {
            var existingPlugin = dbPlugins.FirstOrDefault(p => p.Id == plugin.Id);
            
            if (existingPlugin == null)
            {
                // Add new plugin
                plugin.IsEnabled = _autoEnableNewPlugins;
                plugin.InstalledDate = DateTime.UtcNow;
                
                await _dbContext.Plugins.AddAsync(plugin, cancellationToken);
                
                _logger.LogInformation("Added new plugin to database: {PluginId}, {PluginName}", 
                    plugin.Id, plugin.Name);
            }
            else
            {
                // Update existing plugin
                existingPlugin.Name = plugin.Name;
                existingPlugin.Version = plugin.Version;
                existingPlugin.AssemblyPath = plugin.AssemblyPath;
                
                _logger.LogDebug("Updated existing plugin: {PluginId}, {PluginName}", 
                    plugin.Id, plugin.Name);
            }
        }
        
        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    // Custom load context to avoid locking assemblies
    private class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext() : base(isCollectible: true)
        {
        }
        
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null; // We'll load directly from the stream
        }
    }
}
