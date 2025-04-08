using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace FluentCMS.Infrastructure.Host.Mvc
{
    // Discovers controller types from plugin assemblies
    public class PluginControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // Find controller types from plugin assemblies
            foreach (var part in parts)
            {
                if (part is AssemblyPart assemblyPart)
                {
                    var assembly = assemblyPart.Assembly;
                    
                    // Skip core assemblies - only process plugin assemblies
                    if (IsPluginAssembly(assembly))
                    {
                        var candidates = assembly.GetExportedTypes()
                            .Where(t => !t.IsAbstract && !t.IsInterface)
                            .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                            .Where(t => typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t));
                        
                        foreach (var candidate in candidates)
                        {
                            feature.Controllers.Add(candidate.GetTypeInfo());
                        }
                    }
                }
            }
        }

        // Determine if an assembly is a plugin assembly
        private bool IsPluginAssembly(Assembly assembly)
        {
            // Skip known infrastructure assemblies
            if (assembly.FullName.StartsWith("FluentCMS.Infrastructure"))
                return false;
                
            if (assembly.FullName.StartsWith("Microsoft"))
                return false;
                
            if (assembly.FullName.StartsWith("System"))
                return false;
                
            // Add more exclusions as needed
            
            return true;
        }
    }
}
