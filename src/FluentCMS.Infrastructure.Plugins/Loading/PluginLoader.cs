using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Core.Contracts;
using FluentCMS.Infrastructure.Storage.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Infrastructure.Plugins.Loading
{
    public class PluginLoader : IPluginLoader, IDisposable
    {
        private readonly ConcurrentDictionary<string, LoadedPluginInfo> _loadedPlugins;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PluginLoader> _logger;

        public PluginLoader(IServiceProvider serviceProvider, ILogger<PluginLoader> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _loadedPlugins = new ConcurrentDictionary<string, LoadedPluginInfo>();
        }

        public async Task<IPlugin> LoadPlugin(PluginMetadata metadata, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Loading plugin: {PluginId}, {PluginName}, {PluginVersion}",
                metadata.Id, metadata.Name, metadata.Version);

            if (_loadedPlugins.TryGetValue(metadata.Id, out var loadedPlugin))
            {
                _logger.LogDebug("Plugin already loaded: {PluginId}", metadata.Id);
                return loadedPlugin.Plugin;
            }

            if (!File.Exists(metadata.AssemblyPath))
            {
                _logger.LogError("Plugin assembly file not found: {AssemblyPath}", metadata.AssemblyPath);
                return null;
            }

            try
            {
                // Create load context for plugin assembly
                var loadContext = new PluginLoadContext(metadata.AssemblyPath);
                
                // Load the assembly
                var assembly = loadContext.LoadFromAssemblyPath(metadata.AssemblyPath);
                _logger.LogDebug("Loaded assembly: {AssemblyName}", assembly.FullName);

                // Find the plugin type
                var pluginType = assembly.GetTypes()
                    .FirstOrDefault(t => 
                        !t.IsAbstract && 
                        typeof(IPlugin).IsAssignableFrom(t));
                    
                if (pluginType == null)
                {
                    _logger.LogError("No plugin type found in assembly: {AssemblyPath}", metadata.AssemblyPath);
                    return null;
                }

                // Create plugin instance
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                
                // Verify plugin metadata matches
                if (plugin.Id != metadata.Id)
                {
                    _logger.LogWarning("Plugin ID mismatch: expected {ExpectedId}, found {ActualId}", 
                        metadata.Id, plugin.Id);
                }

                // Initialize plugin
                await plugin.Initialize(_serviceProvider, cancellationToken);
                
                // Store loaded plugin info
                var info = new LoadedPluginInfo
                {
                    Plugin = plugin,
                    Assembly = assembly,
                    LoadContext = loadContext
                };
                
                _loadedPlugins[plugin.Id] = info;
                
                _logger.LogInformation("Plugin loaded and initialized: {PluginId}", plugin.Id);
                
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plugin: {PluginId}, {AssemblyPath}", 
                    metadata.Id, metadata.AssemblyPath);
                return null;
            }
        }

        public async Task UnloadPlugin(string pluginId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Unloading plugin: {PluginId}", pluginId);

            if (!_loadedPlugins.TryRemove(pluginId, out var loadedPlugin))
            {
                _logger.LogWarning("Plugin not loaded: {PluginId}", pluginId);
                return;
            }

            try
            {
                // Shutdown plugin
                await loadedPlugin.Plugin.Shutdown(cancellationToken);
                
                // Unload assembly context
                loadedPlugin.LoadContext.Unload();
                
                _logger.LogInformation("Plugin unloaded: {PluginId}", pluginId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading plugin: {PluginId}", pluginId);
            }
        }

        public IPlugin GetPlugin(string pluginId)
        {
            _logger.LogDebug("Getting plugin: {PluginId}", pluginId);
            
            if (_loadedPlugins.TryGetValue(pluginId, out var loadedPlugin))
            {
                return loadedPlugin.Plugin;
            }
            
            _logger.LogDebug("Plugin not loaded: {PluginId}", pluginId);
            return null;
        }

        public IEnumerable<IPlugin> GetActivePlugins()
        {
            _logger.LogDebug("Getting all active plugins");
            return _loadedPlugins.Values.Select(p => p.Plugin);
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing plugin loader, unloading all plugins");
            
            // Create a list of plugin IDs to avoid collection modified exceptions
            var pluginIds = _loadedPlugins.Keys.ToList();
            
            foreach (var pluginId in pluginIds)
            {
                try
                {
                    // Use fire and forget for shutdown to avoid blocking
                    UnloadPlugin(pluginId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unloading plugin during disposal: {PluginId}", pluginId);
                }
            }
        }

        // Class to track loaded plugin info
        private class LoadedPluginInfo
        {
            public IPlugin Plugin { get; set; }
            public Assembly Assembly { get; set; }
            public PluginLoadContext LoadContext { get; set; }
        }
    }
}
