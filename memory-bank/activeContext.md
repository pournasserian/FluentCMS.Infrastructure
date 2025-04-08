# Active Context: FluentCMS Plugin System

## Current Focus
- Initial project setup and architecture design
- Defining core interfaces and contracts for the plugin system
- Establishing foundational components:
  - Plugin discovery mechanism
  - Plugin loading/unloading infrastructure
  - Plugin metadata management

## Recent Changes
- Created initial memory bank documentation
- Defined architectural approach based on README specifications
- Outlined plugin contract interfaces
- Established design patterns for system components

## Next Steps
1. Create solution structure and project organization
2. Implement core plugin contracts:
   - IPlugin base interface
   - IPluginStartup for service configuration
   - IPluginInstaller for first-time setup
   - IBackgroundTaskProvider for background services
3. Develop plugin discovery service
4. Implement plugin loading mechanism with assembly isolation
5. Create plugin registry for tracking active plugins
6. Set up plugin communication system

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
