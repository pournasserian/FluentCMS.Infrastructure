using System;
using System.Linq;
using FluentCMS.Infrastructure.Core.Contracts;
using FluentCMS.Infrastructure.Host.Middleware;
using FluentCMS.Infrastructure.Plugins.Loading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Infrastructure.Host.Extensions
{
    // Extensions for configuring plugin middleware
    public static class ApplicationBuilderExtensions
    {
        // Adds all plugin middleware to the pipeline
        public static IApplicationBuilder UsePlugins(this IApplicationBuilder app, IPluginLoader pluginLoader)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IApplicationBuilder>>();
            logger.LogInformation("Configuring middleware for plugins");
            
            // Add middleware for all active plugins
            foreach (var plugin in pluginLoader.GetActivePlugins())
            {
                if (plugin is IPluginStartup startup)
                {
                    try
                    {
                        logger.LogInformation("Configuring middleware for plugin: {PluginId}", plugin.Id);
                        
                        // Create plugin-specific application builder
                        var pluginAppBuilder = new PluginApplicationBuilder(app, plugin.Id);
                        
                        // Call plugin's Configure method
                        startup.Configure(pluginAppBuilder);
                        
                        logger.LogInformation("Successfully configured middleware for plugin: {PluginId}", plugin.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error configuring middleware for plugin: {PluginId}", plugin.Id);
                    }
                }
            }
            
            return app;
        }
        
        // Configures plugin endpoints
        public static IEndpointRouteBuilder MapPluginEndpoints(this IEndpointRouteBuilder endpoints, IPluginLoader pluginLoader)
        {
            var logger = endpoints.ServiceProvider.GetRequiredService<ILogger<IEndpointRouteBuilder>>();
            logger.LogInformation("Mapping endpoints for plugins");
            
            foreach (var plugin in pluginLoader.GetActivePlugins())
            {
                if (plugin is IPluginStartup startup)
                {
                    try
                    {
                        // Check if the plugin startup has ConfigureEndpoints method
                        var configureEndpointsMethod = startup.GetType().GetMethod("ConfigureEndpoints");
                        if (configureEndpointsMethod != null)
                        {
                            logger.LogInformation("Mapping endpoints for plugin: {PluginId}", plugin.Id);
                            
                            // Invoke ConfigureEndpoints if it exists
                            configureEndpointsMethod.Invoke(startup, new object[] { endpoints });
                            
                            logger.LogInformation("Successfully mapped endpoints for plugin: {PluginId}", plugin.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error mapping endpoints for plugin: {PluginId}", plugin.Id);
                    }
                }
            }
            
            return endpoints;
        }
    }
}
