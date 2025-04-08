using System.Threading;
using System.Threading.Tasks;

namespace FluentCMS.Infrastructure.Core.Contracts
{
    // Interface for first-time setup and installation of plugins
    public interface IPluginInstaller
    {
        // Check if plugin is already installed
        Task<bool> IsInstalled(CancellationToken cancellationToken = default);
        
        // Perform first-time setup (database, resources, etc.)
        Task<bool> Install(CancellationToken cancellationToken = default);
        
        // Clean up resources and remove plugin artifacts
        Task<bool> Uninstall(CancellationToken cancellationToken = default);
    }
}
