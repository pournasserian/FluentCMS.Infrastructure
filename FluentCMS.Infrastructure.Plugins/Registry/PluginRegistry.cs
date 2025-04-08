using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Storage.Data;
using FluentCMS.Infrastructure.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Infrastructure.Plugins.Registry
{
    public class PluginRegistry : IPluginRegistry
    {
        private readonly PluginDbContext _dbContext;
        private readonly ILogger<PluginRegistry> _logger;

        public PluginRegistry(PluginDbContext dbContext, ILogger<PluginRegistry> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<PluginMetadata>> GetAllPlugins(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving all plugins from database");
            return await _dbContext.Plugins.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PluginMetadata>> GetActivePlugins(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving active plugins from database");
            return await _dbContext.Plugins
                .Where(p => p.IsEnabled)
                .ToListAsync(cancellationToken);
        }

        public async Task<PluginMetadata> GetPluginById(string pluginId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Retrieving plugin with ID: {PluginId}", pluginId);
            return await _dbContext.Plugins
                .FirstOrDefaultAsync(p => p.Id == pluginId, cancellationToken);
        }

        public async Task<bool> UpdatePluginStatus(string pluginId, bool isEnabled, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating plugin status: {PluginId}, IsEnabled: {IsEnabled}", pluginId, isEnabled);
            
            var plugin = await _dbContext.Plugins
                .FirstOrDefaultAsync(p => p.Id == pluginId, cancellationToken);
                
            if (plugin == null)
            {
                _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
                return false;
            }

            plugin.IsEnabled = isEnabled;
            
            if (isEnabled)
            {
                plugin.LastEnabledDate = System.DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Plugin status updated successfully: {PluginId}", pluginId);
            return true;
        }
    }
}
