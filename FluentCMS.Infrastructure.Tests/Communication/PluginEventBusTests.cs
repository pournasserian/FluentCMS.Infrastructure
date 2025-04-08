using System;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Plugins.Communication;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentCMS.Infrastructure.Tests.Communication
{
    public class PluginEventBusTests
    {
        private readonly Mock<ILogger<PluginEventBus>> _loggerMock;
        private readonly PluginEventBus _eventBus;

        public PluginEventBusTests()
        {
            _loggerMock = new Mock<ILogger<PluginEventBus>>();
            _eventBus = new PluginEventBus(_loggerMock.Object);
        }

        [Fact]
        public async Task Publish_WithNoSubscribers_ShouldNotThrowException()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test Message" };

            // Act & Assert
            await _eventBus.Publish(testEvent);
            // No exception means test passes
        }

        [Fact]
        public async Task Publish_WithSubscriber_ShouldInvokeHandler()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test Message" };
            var handlerInvoked = false;

            Task Handler(TestEvent @event, CancellationToken cancellationToken)
            {
                handlerInvoked = true;
                Assert.Equal("Test Message", @event.Message);
                return Task.CompletedTask;
            }

            await _eventBus.Subscribe<TestEvent>(Handler);

            // Act
            await _eventBus.Publish(testEvent);

            // Assert
            Assert.True(handlerInvoked);
        }

        [Fact]
        public async Task SubscribeAndDispose_ShouldNotInvokeHandlerAfterDisposal()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test Message" };
            var handlerInvokeCount = 0;

            Task Handler(TestEvent @event, CancellationToken cancellationToken)
            {
                handlerInvokeCount++;
                return Task.CompletedTask;
            }

            // Act
            var subscription = await _eventBus.Subscribe<TestEvent>(Handler);
            await _eventBus.Publish(testEvent);
            
            // Dispose subscription
            subscription.Dispose();
            await _eventBus.Publish(testEvent);

            // Assert
            Assert.Equal(1, handlerInvokeCount);
        }

        [Fact]
        public async Task Publish_WithMultipleSubscribers_ShouldInvokeAllHandlers()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test Message" };
            var handler1Invoked = false;
            var handler2Invoked = false;

            Task Handler1(TestEvent @event, CancellationToken cancellationToken)
            {
                handler1Invoked = true;
                return Task.CompletedTask;
            }

            Task Handler2(TestEvent @event, CancellationToken cancellationToken)
            {
                handler2Invoked = true;
                return Task.CompletedTask;
            }

            await _eventBus.Subscribe<TestEvent>(Handler1);
            await _eventBus.Subscribe<TestEvent>(Handler2);

            // Act
            await _eventBus.Publish(testEvent);

            // Assert
            Assert.True(handler1Invoked);
            Assert.True(handler2Invoked);
        }

        [Fact]
        public async Task Publish_WithExceptionInHandler_ShouldContinueProcessingOtherHandlers()
        {
            // Arrange
            var testEvent = new TestEvent { Message = "Test Message" };
            var handler2Invoked = false;

            Task Handler1(TestEvent @event, CancellationToken cancellationToken)
            {
                throw new Exception("Test exception");
            }

            Task Handler2(TestEvent @event, CancellationToken cancellationToken)
            {
                handler2Invoked = true;
                return Task.CompletedTask;
            }

            await _eventBus.Subscribe<TestEvent>(Handler1);
            await _eventBus.Subscribe<TestEvent>(Handler2);

            // Act
            await _eventBus.Publish(testEvent);

            // Assert
            Assert.True(handler2Invoked);
        }

        // Test event class
        private class TestEvent
        {
            public string Message { get; set; }
        }
    }
}
