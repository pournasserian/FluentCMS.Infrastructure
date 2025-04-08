using System;
using System.IO;
using System.Threading.Tasks;
using FluentCMS.Infrastructure.Host.Extensions;
using FluentCMS.Infrastructure.Plugins.Discovery;
using FluentCMS.Infrastructure.Plugins.Loading;
using FluentCMS.Infrastructure.Plugins.Registry;
using FluentCMS.Infrastructure.Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentCMS.Host.Sample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create and configure host
            var host = CreateHostBuilder(args).Build();

            // Ensure database is created
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PluginDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
                
                Console.WriteLine("Database created or exists.");
            }

            // Discover and load plugins
            await ManagePlugins(host.Services);

            // Keep the application running
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add FluentCMS plugin system
                    services.AddFluentCmsPluginSystem(hostContext.Configuration);
                });

        static async Task ManagePlugins(IServiceProvider serviceProvider)
        {
            try
            {
                // Get plugin services
                var discoveryService = serviceProvider.GetRequiredService<IPluginDiscoveryService>();
                var registry = serviceProvider.GetRequiredService<IPluginRegistry>();
                var loader = serviceProvider.GetRequiredService<IPluginLoader>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Discover plugins
                logger.LogInformation("Discovering plugins...");
                var plugins = await discoveryService.DiscoverPlugins();
                logger.LogInformation("Discovered {Count} plugins", plugins.Count());

                // Load and initialize enabled plugins
                var enabledPlugins = await registry.GetActivePlugins();
                foreach (var metadata in enabledPlugins)
                {
                    logger.LogInformation("Loading plugin: {PluginId}", metadata.Id);
                    var plugin = await loader.LoadPlugin(metadata);
                    
                    if (plugin != null)
                    {
                        logger.LogInformation("Plugin loaded: {PluginId}, {PluginName}, {PluginVersion}",
                            plugin.Id, plugin.Name, plugin.Version);
                    }
                }

                // Show a menu for plugin management
                var exit = false;
                while (!exit)
                {
                    Console.WriteLine("\nPlugin Management Menu:");
                    Console.WriteLine("1. List all plugins");
                    Console.WriteLine("2. Enable plugin");
                    Console.WriteLine("3. Disable plugin");
                    Console.WriteLine("4. Register new plugin");
                    Console.WriteLine("5. Exit");
                    Console.Write("Enter option: ");
                    
                    var option = Console.ReadLine();
                    
                    switch (option)
                    {
                        case "1":
                            await ListPlugins(registry);
                            break;
                        case "2":
                            await EnablePlugin(registry, loader);
                            break;
                        case "3":
                            await DisablePlugin(registry, loader);
                            break;
                        case "4":
                            await RegisterPlugin(discoveryService);
                            break;
                        case "5":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error managing plugins: {ex.Message}");
            }
        }

        static async Task ListPlugins(IPluginRegistry registry)
        {
            var plugins = await registry.GetAllPlugins();
            
            Console.WriteLine("\nAll Plugins:");
            foreach (var plugin in plugins)
            {
                Console.WriteLine($"{plugin.Id} - {plugin.Name} ({plugin.Version}) - {(plugin.IsEnabled ? "Enabled" : "Disabled")}");
            }
        }

        static async Task EnablePlugin(IPluginRegistry registry, IPluginLoader loader)
        {
            Console.Write("Enter plugin ID to enable: ");
            var pluginId = Console.ReadLine();
            
            var metadata = await registry.GetPluginById(pluginId);
            if (metadata == null)
            {
                Console.WriteLine("Plugin not found");
                return;
            }
            
            if (metadata.IsEnabled)
            {
                Console.WriteLine("Plugin is already enabled");
                return;
            }
            
            var result = await registry.UpdatePluginStatus(pluginId, true);
            if (result)
            {
                Console.WriteLine("Plugin enabled successfully");
                
                // Load plugin
                await loader.LoadPlugin(metadata);
                Console.WriteLine("Plugin loaded");
            }
            else
            {
                Console.WriteLine("Failed to enable plugin");
            }
        }

        static async Task DisablePlugin(IPluginRegistry registry, IPluginLoader loader)
        {
            Console.Write("Enter plugin ID to disable: ");
            var pluginId = Console.ReadLine();
            
            var metadata = await registry.GetPluginById(pluginId);
            if (metadata == null)
            {
                Console.WriteLine("Plugin not found");
                return;
            }
            
            if (!metadata.IsEnabled)
            {
                Console.WriteLine("Plugin is already disabled");
                return;
            }
            
            var result = await registry.UpdatePluginStatus(pluginId, false);
            if (result)
            {
                Console.WriteLine("Plugin disabled successfully");
                
                // Unload plugin
                await loader.UnloadPlugin(pluginId);
                Console.WriteLine("Plugin unloaded");
            }
            else
            {
                Console.WriteLine("Failed to disable plugin");
            }
        }

        static async Task RegisterPlugin(IPluginDiscoveryService discoveryService)
        {
            Console.Write("Enter path to plugin assembly: ");
            var path = Console.ReadLine();
            
            var metadata = await discoveryService.RegisterPlugin(path);
            if (metadata != null)
            {
                Console.WriteLine($"Plugin registered: {metadata.Id} - {metadata.Name} ({metadata.Version})");
            }
            else
            {
                Console.WriteLine("Failed to register plugin");
            }
        }
    }
}
