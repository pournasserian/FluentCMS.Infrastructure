using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Core.Contracts;
using FluentCMS.Infrastructure.Plugins.Loading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Infrastructure.Host.BackgroundTasks
{
    // Manages background tasks from plugins
    public class PluginBackgroundTaskManager : IHostedService
    {
        private readonly IPluginLoader _pluginLoader;
        private readonly ILogger<PluginBackgroundTaskManager> _logger;
        private readonly List<BackgroundTaskInfo> _hostedServices = new List<BackgroundTaskInfo>();

        public PluginBackgroundTaskManager(
            IPluginLoader pluginLoader,
            ILogger<PluginBackgroundTaskManager> logger)
        {
            _pluginLoader = pluginLoader;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting plugin background tasks");
            
            foreach (var plugin in _pluginLoader.GetActivePlugins())
            {
                if (plugin is IBackgroundTaskProvider taskProvider)
                {
                    try
                    {
                        _logger.LogInformation("Processing background tasks for plugin: {PluginId}", plugin.Id);
                        
                        var services = taskProvider.GetBackgroundServices();
                        foreach (var service in services)
                        {
                            var taskInfo = new BackgroundTaskInfo
                            {
                                PluginId = plugin.Id,
                                Service = service
                            };
                            
                            _hostedServices.Add(taskInfo);
                            await service.StartAsync(cancellationToken);
                            
                            _logger.LogInformation("Started background service {ServiceType} for plugin {PluginId}", 
                                service.GetType().Name, plugin.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error starting background tasks for plugin: {PluginId}", plugin.Id);
                    }
                }
            }
            
            _logger.LogInformation("Completed starting plugin background tasks");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping plugin background tasks");
            
            // Stop services in reverse order to respect potential dependencies
            for (int i = _hostedServices.Count - 1; i >= 0; i--)
            {
                var taskInfo = _hostedServices[i];
                try
                {
                    _logger.LogInformation("Stopping background service for plugin {PluginId}", taskInfo.PluginId);
                    await taskInfo.Service.StopAsync(cancellationToken);
                    _logger.LogInformation("Successfully stopped background service for plugin {PluginId}", taskInfo.PluginId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping background service for plugin {PluginId}", taskInfo.PluginId);
                }
            }
            
            _hostedServices.Clear();
            _logger.LogInformation("Completed stopping all plugin background tasks");
        }

        // Helper class to keep track of services and their associated plugins
        private class BackgroundTaskInfo
        {
            public string PluginId { get; set; }
            public IHostedService Service { get; set; }
        }
    }
}
