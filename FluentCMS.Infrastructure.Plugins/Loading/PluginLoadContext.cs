using System.Reflection;
using System.Runtime.Loader;

namespace FluentCMS.Infrastructure.Plugins.Loading
{
    // Assembly load context for loading plugin assemblies in isolation
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        // Override assembly loading to use the resolver for dependencies
        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Try to resolve the assembly path from the plugin dependencies
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            // Fall back to default loading if not found
            return null;
        }
    }
}
