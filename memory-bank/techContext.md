# Technical Context: FluentCMS Plugin System

## Technology Stack

### Core Technologies
- **ASP.NET Core**: Framework for building web applications and APIs
- **.NET 8**: Latest .NET framework version for high performance and cross-platform support
- **C#**: Primary programming language
- **Entity Framework Core**: ORM for database access
- **SQL Server**: Primary database for production

### Supporting Libraries
- **MediatR**: For implementing mediator pattern in plugin communication
- **Scrutor**: For advanced service registration and assembly scanning
- **Microsoft.Extensions.DependencyInjection**: For dependency injection
- **Microsoft.Extensions.Hosting**: For background service support
- **System.Reflection.MetadataLoadContext**: For loading assemblies in isolated contexts

## Development Environment
- **Visual Studio 2022** or **VS Code** with C# extensions
- **.NET 8 SDK**
- **SQL Server** (local or containerized)
- **Git** for version control

## Technical Constraints

### Assembly Loading
- Plugin assemblies must be loadable dynamically at runtime
- Assembly loading must consider dependency resolution
- Proper isolation to prevent version conflicts
- Support for unloading assemblies cleanly

### Plugin Isolation
- Plugin services should be isolated from each other
- Failures in one plugin should not affect others
- Resource usage should be monitored and limited
- Security boundaries must be maintained

### Performance Requirements
- Plugin loading/unloading should complete in under 3 seconds
- Plugin overhead should be minimal in HTTP request pipeline
- Memory leaks must be prevented during plugin lifecycle
- Background tasks should respect cancellation and shutdown signals

### Database Considerations
- Support for plugin-specific database schemas
- Migration management for plugin database changes
- Connection pooling for efficient database access
- Transaction support across plugin boundaries

## Technical Approaches

### Assembly Loading Strategy
```csharp
// Using AssemblyLoadContext for loadable/unloadable assemblies
public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }
}
```

### Dependency Injection Integration
```csharp
// Registering plugin services with service collection
public void ConfigureServices(IServiceCollection services)
{
    // Get all active plugins
    var activePlugins = _pluginRegistry.GetActivePlugins();
    
    // Configure each plugin's services
    foreach (var plugin in activePlugins)
    {
        if (plugin is IPluginStartup startup)
        {
            // Let plugin register its own services
            startup.ConfigureServices(services);
        }
    }
}
```

### Service Isolation Approach
```csharp
// Using named service registration for isolation
services.AddScoped<IMyService, MyServiceImplementation>(serviceProvider => 
    new MyServiceImplementation(), $"plugin:{pluginId}");

// Service accessor for retrieving plugin-specific services
public T GetPluginService<T>(string pluginId) where T : class
{
    return _serviceProvider.GetService<T>($"plugin:{pluginId}");
}
```

### Background Task Management
```csharp
// Background service registration and management
public IEnumerable<IHostedService> GetBackgroundServices()
{
    yield return new TimedHostedService(_serviceProvider);
    yield return new QueuedHostedService(_serviceProvider);
}

// Startup registration
services.AddHostedService<PluginBackgroundServiceManager>();
```

## Testing Strategy
- Unit tests for core components
- Integration tests for plugin loading and communication
- Load testing for performance validation
- Sandbox testing for security validation

## Deployment Considerations
- Plugin assemblies stored in designated directory
- Database migrations applied on plugin activation
- Configuration through environment variables or configuration files
- Health checks for plugin status monitoring
