using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCMS.Infrastructure.Core.Communication
{
    // Interface for plugin communication using event bus pattern
    public interface IPluginEventBus
    {
        // Publish an event to all subscribers
        Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
            where TEvent : class;
        
        // Subscribe to an event type with a handler
        Task<IDisposable> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) 
            where TEvent : class;
    }
}
