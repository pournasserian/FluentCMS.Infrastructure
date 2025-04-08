using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Core.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.Plugin
{
    public class SamplePlugin : IPlugin, IPluginStartup, IBackgroundTaskProvider
    {
        private ILogger<SamplePlugin> _logger;
        private ILoggerFactory _loggerFactory;
        
        public string Id => "sample-plugin";
        public string Name => "Sample Plugin";
        public string Version => "1.0.0";

        public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var factory = serviceProvider.GetService<ILoggerFactory>();
            _loggerFactory = factory;
            _logger = factory?.CreateLogger<SamplePlugin>();
            _logger?.LogInformation("Sample plugin initializing");
            
            // Simulate some initialization work
            await Task.Delay(100, cancellationToken);
            
            _logger?.LogInformation("Sample plugin initialized successfully");
        }

        public async Task Shutdown(CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Sample plugin shutting down");
            
            // Simulate some cleanup work
            await Task.Delay(100, cancellationToken);
            
            _logger?.LogInformation("Sample plugin shut down successfully");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register plugin services
            services.AddTransient<ISampleService, SampleService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            // No middleware configuration for simplicity
        }
        
        // Provide background services
        public IEnumerable<IHostedService> GetBackgroundServices()
        {
            yield return new SampleBackgroundService(_loggerFactory?.CreateLogger<SampleBackgroundService>());
        }
    }

    // Sample plugin service interface
    public interface ISampleService
    {
        string GetMessage();
    }

    // Sample plugin service implementation
    public class SampleService : ISampleService
    {
        public string GetMessage()
        {
            return "Hello from Sample Plugin!";
        }
    }
}
