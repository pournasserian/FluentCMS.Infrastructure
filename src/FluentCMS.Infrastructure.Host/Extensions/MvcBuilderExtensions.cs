using System;
using System.Reflection;
using FluentCMS.Infrastructure.Host.Mvc;
using FluentCMS.Infrastructure.Plugins.Loading;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Infrastructure.Host.Extensions
{
    // Extensions for registering plugin controllers with MVC
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddPluginControllers(this IMvcBuilder builder, IPluginLoader pluginLoader)
        {
            // Register plugin controller convention
            builder.ConfigureApplicationPartManager(manager => 
            {
                // Add controller feature provider for discovering controllers in plugin assemblies
                manager.FeatureProviders.Add(new PluginControllerFeatureProvider());
                
                // Add application parts for each active plugin
                foreach (var plugin in pluginLoader.GetActivePlugins())
                {
                    // Get the assembly containing the plugin
                    var assembly = plugin.GetType().Assembly;
                    
                    // Add controllers from this assembly
                    var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
                    {
                        manager.ApplicationParts.Add(applicationPart);
                    }
                }
            });
            
            // Add plugin controller route convention
            builder.AddMvcOptions(options => 
            {
                options.Conventions.Add(new PluginControllerRouteConvention());
            });
            
            return builder;
        }
    }
}
