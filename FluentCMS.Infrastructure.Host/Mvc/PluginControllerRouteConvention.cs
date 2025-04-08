using System;
using System.Reflection;
using FluentCMS.Infrastructure.Core.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FluentCMS.Infrastructure.Host.Mvc
{
    // Convention for adding plugin-specific route prefixes to controllers
    public class PluginControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.Assembly == GetType().Assembly)
            {
                // Skip controllers in the infrastructure assembly
                return;
            }

            // Get plugin ID from assembly
            var pluginId = GetPluginIdFromController(controller);
            
            if (!string.IsNullOrEmpty(pluginId))
            {
                // Add route prefix with plugin ID
                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel != null)
                    {
                        // Modify existing attribute route to include plugin prefix
                        var pluginRouteModel = new AttributeRouteModel { Template = $"api/plugins/{pluginId}" };
                        selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                            pluginRouteModel,
                            selector.AttributeRouteModel
                        );
                    }
                    else
                    {
                        // Add new attribute route with plugin-specific convention
                        selector.AttributeRouteModel = new AttributeRouteModel
                        {
                            Template = $"api/plugins/{pluginId}/[controller]/[action]"
                        };
                    }
                }
            }
        }
        
        // Extract plugin ID from controller
        private string GetPluginIdFromController(ControllerModel controller)
        {
            var assembly = controller.ControllerType.Assembly;
            
            // First check if there's a plugin type in the assembly
            foreach (var type in assembly.GetExportedTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    try
                    {
                        // Try to create instance to get plugin ID
                        var plugin = (IPlugin)Activator.CreateInstance(type);
                        return plugin.Id;
                    }
                    catch
                    {
                        // Failed to create instance, try another approach
                    }
                }
            }
            
            // Fallback to assembly name-based ID
            var assemblyName = assembly.GetName().Name;
            return assemblyName.Replace(".", "-").ToLowerInvariant();
        }
    }
}
