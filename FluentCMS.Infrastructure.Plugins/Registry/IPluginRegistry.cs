using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Storage.Models;

namespace FluentCMS.Infrastructure.Plugins.Registry
{
    public interface IPluginRegistry
    {
        // Get all registered plugins (active and inactive)
        Task<IEnumerable<PluginMetadata>> GetAllPlugins(CancellationToken cancellationToken = default);
        
        // Get only active plugins
        Task<IEnumerable<PluginMetadata>> GetActivePlugins(CancellationToken cancellationToken = default);
        
        // Get a specific plugin by its ID
        Task<PluginMetadata> GetPluginById(string pluginId, CancellationToken cancellationToken = default);
        
        // Update a plugin's enabled status
        Task<bool> UpdatePluginStatus(string pluginId, bool isEnabled, CancellationToken cancellationToken = default);
    }
}
