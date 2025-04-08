using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Core.Contracts;
using FluentCMS.Infrastructure.Storage.Models;

namespace FluentCMS.Infrastructure.Plugins.Loading
{
    public interface IPluginLoader
    {
        // Load a plugin from the provided metadata
        Task<IPlugin> LoadPlugin(PluginMetadata metadata, CancellationToken cancellationToken = default);
        
        // Unload a plugin by its ID
        Task UnloadPlugin(string pluginId, CancellationToken cancellationToken = default);
        
        // Get a specific loaded plugin by its ID
        IPlugin GetPlugin(string pluginId);
        
        // Get all currently active plugins
        IEnumerable<IPlugin> GetActivePlugins();
    }
}
