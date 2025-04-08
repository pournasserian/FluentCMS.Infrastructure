using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Storage.Models;

namespace FluentCMS.Infrastructure.Plugins.Discovery
{
    public interface IPluginDiscoveryService
    {
        // Discover all plugins in the plugins directory
        Task<IEnumerable<PluginMetadata>> DiscoverPlugins(CancellationToken cancellationToken = default);
        
        // Register a new plugin from an assembly path
        Task<PluginMetadata> RegisterPlugin(string assemblyPath, CancellationToken cancellationToken = default);
        
        // Enable a plugin by its ID
        Task<bool> EnablePlugin(string pluginId, CancellationToken cancellationToken = default);
        
        // Disable a plugin by its ID
        Task<bool> DisablePlugin(string pluginId, CancellationToken cancellationToken = default);
    }
}
