# Active Context: FluentCMS Plugin System

## Current Focus
- Implementing concrete service classes for the plugin system
- Developing plugin discovery and loading functionality
- Creating a sample plugin to validate the system
- Setting up database migrations for plugin metadata

## Recent Changes
- Created solution structure with 5 projects (Core, Plugins, Storage, Host, Tests)
- Implemented core plugin contracts (IPlugin, IPluginStartup, IPluginInstaller, IBackgroundTaskProvider)
- Created plugin metadata model and database context
- Implemented plugin event bus for inter-plugin communication with tests
- Set up interfaces for plugin discovery, loading, and registry
- Created extension methods for registering plugin system with host

## Next Steps
1. Implement concrete service classes:
   - PluginDiscoveryService
   - PluginRegistry
   - PluginLoader
2. Develop database migration setup for plugin metadata
3. Create a sample plugin to validate the system
4. Add controllers for plugin management API
5. Implement middleware integration for plugins
6. Set up background task management for plugins

## Active Decisions

### Assembly Loading Approach
- Using `AssemblyLoadContext` with `isCollectible: true` for proper unloading
- Plugin assemblies will be loaded in isolated contexts
- Dependencies will be resolved through `AssemblyDependencyResolver`

### Plugin Service Registration
- Plugins will register services through `IPluginStartup.ConfigureServices`
- Service lifetimes will respect DI container's standard patterns
- Service isolation will use named registrations where appropriate

### API Integration Strategy
- Plugins will define controllers in a designated namespace
- Routes will be automatically prefixed with plugin identifier
- Standard RESTful conventions will be followed
- API versioning will be supported

### Plugin Communication Pattern
- Using event-based communication through `IPluginEventBus`
- Events as POCOs that can be serialized/deserialized
- Support for both synchronous and asynchronous subscribers
- Error handling in message processing to prevent cascade failures

## Important Patterns and Preferences
- Favor composition over inheritance in plugin design
- Use dependency injection consistently throughout the system
- Follow async/await patterns for I/O-bound operations
- Use cancellation tokens for long-running operations
- Implement proper resource disposal for loaded plugins

## Implementation Insights
- Plugin isolation is challenging but critical for system stability
- Assembly loading contexts must be managed carefully to prevent memory leaks
- Database access from plugins needs special consideration for connection management
- Exception handling across plugin boundaries requires careful design
