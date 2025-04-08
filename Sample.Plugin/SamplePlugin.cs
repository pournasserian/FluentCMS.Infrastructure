using System;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Core.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample.Plugin
{
    public class SamplePlugin : IPlugin, IPluginStartup
    {
        private ILogger<SamplePlugin> _logger;
        
        public string Id => "sample-plugin";
        public string Name => "Sample Plugin";
        public string Version => "1.0.0";

        public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<SamplePlugin>();
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
            // Configure middleware if needed
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
