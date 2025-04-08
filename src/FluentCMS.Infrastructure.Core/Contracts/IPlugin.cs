namespace FluentCMS.Infrastructure.Core.Contracts;

// Base interface for all plugins
public interface IPlugin
{
    // Core plugin metadata
    string Id { get; }
    string Name { get; }
    string Version { get; }
    
    // Plugin lifecycle methods
    Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    Task Shutdown(CancellationToken cancellationToken = default);
}
