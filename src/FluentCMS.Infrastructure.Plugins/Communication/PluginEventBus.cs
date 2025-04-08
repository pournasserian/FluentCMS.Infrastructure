using System.Collections.Concurrent;
using FluentCMS.Infrastructure.Core.Communication;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Infrastructure.Plugins.Communication;

public class PluginEventBus : IPluginEventBus
{
    private readonly ILogger<PluginEventBus> _logger;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<Delegate>> _handlers;

    public PluginEventBus(ILogger<PluginEventBus> logger)
    {
        _logger = logger;
        _handlers = new ConcurrentDictionary<Type, ConcurrentBag<Delegate>>();
    }

    public Task Publish<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();
        _logger.LogDebug("Publishing event of type {EventType}", eventType.Name);

        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No handlers registered for event type {EventType}", eventType.Name);
            return Task.CompletedTask;
        }

        var tasks = new Task[handlers.Count];
        int i = 0;

        foreach (var handler in handlers)
        {
            var handlerFunc = (Func<TEvent, CancellationToken, Task>)handler;
            tasks[i] = InvokeHandlerSafely(handlerFunc, @event, cancellationToken);
            i++;
        }

        return Task.WhenAll(tasks);
    }

    public Task<IDisposable> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where TEvent : class
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var eventType = typeof(TEvent);
        _logger.LogDebug("Subscribing to event of type {EventType}", eventType.Name);

        var handlers = _handlers.GetOrAdd(eventType, _ => new ConcurrentBag<Delegate>());
        handlers.Add(handler);

        // Return a disposable that will unsubscribe when disposed
        return Task.FromResult<IDisposable>(new SubscriptionDisposable<TEvent>(this, handler));
    }

    // Helper method to safely invoke a handler and log any exceptions
    private async Task InvokeHandlerSafely<TEvent>(Func<TEvent, CancellationToken, Task> handler, TEvent @event, CancellationToken cancellationToken) where TEvent : class
    {
        try
        {
            await handler(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event of type {EventType}", typeof(TEvent).Name);
        }
    }

    // Helper method to unsubscribe a handler (used by SubscriptionDisposable)
    private void Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        _logger.LogDebug("Unsubscribing from event of type {EventType}", eventType.Name);

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            // Create a new bag without the specified handler
            var newHandlers = new ConcurrentBag<Delegate>();
            foreach (var existingHandler in handlers)
            {
                if (existingHandler != (Delegate)handler)
                {
                    newHandlers.Add(existingHandler);
                }
            }

            // Replace the old bag with the new one
            _handlers.TryUpdate(eventType, newHandlers, handlers);
        }
    }

    // Disposable class to manage event subscription lifetime
    private class SubscriptionDisposable<TEvent> : IDisposable where TEvent : class
    {
        private readonly PluginEventBus _eventBus;
        private readonly Func<TEvent, CancellationToken, Task> _handler;

        public SubscriptionDisposable(PluginEventBus eventBus, Func<TEvent, CancellationToken, Task> handler)
        {
            _eventBus = eventBus;
            _handler = handler;
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe(_handler);
        }
    }
}
