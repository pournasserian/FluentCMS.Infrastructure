# Active Context: FluentCMS Plugin System

## Current Focus
- Testing and validating the plugin system in real-world scenarios
- Enhancing plugin management capabilities with API controllers
- Adding middleware integration for plugins
- Implementing background task management for plugins

## Recent Changes
- Implemented concrete service classes:
  - PluginDiscoveryService for scanning and registering plugins
  - PluginRegistry for tracking plugin state
  - PluginLoader for loading and unloading plugin assemblies
- Created PluginOptions for system configuration
- Added database migration for plugin metadata
- Created sample plugin and host application for testing
- Updated extension methods for registering plugin system with host

## Next Steps
1. Create API controllers for plugin management
2. Implement middleware integration support for plugins
3. Add background task management for plugins
4. Create UI for plugin management
5. Enhance security features for plugin validation
6. Add support for plugin configuration management

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
