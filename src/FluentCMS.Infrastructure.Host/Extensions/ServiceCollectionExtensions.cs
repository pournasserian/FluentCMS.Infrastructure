using FluentCMS.Infrastructure.Core.Communication;
using FluentCMS.Infrastructure.Host.BackgroundTasks;
using FluentCMS.Infrastructure.Host.Mvc;
using FluentCMS.Infrastructure.Plugins.Communication;
using FluentCMS.Infrastructure.Plugins.Discovery;
using FluentCMS.Infrastructure.Plugins.Loading;
using FluentCMS.Infrastructure.Plugins.Options;
using FluentCMS.Infrastructure.Plugins.Registry;
using FluentCMS.Infrastructure.Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Infrastructure.Host.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentCmsPluginSystem(this IServiceCollection services, IConfiguration configuration)
    {
        // Add plugin options
        services.Configure<PluginOptions>(configuration.GetSection("Plugins"));
        
        // Add database context
        services.AddDbContext<PluginDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register plugin system services
        services.AddSingleton<IPluginEventBus, PluginEventBus>();
        services.AddScoped<IPluginDiscoveryService, PluginDiscoveryService>();
        services.AddScoped<IPluginRegistry, PluginRegistry>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        
        // Add background task manager
        services.AddHostedService<PluginBackgroundTaskManager>();

        return services;
    }
    
    // Extension method to add plugin controller support to MVC
    public static IMvcBuilder AddFluentCmsPluginControllers(this IMvcBuilder builder, IPluginLoader pluginLoader)
    {
        // Add plugin controller feature provider
        builder.ConfigureApplicationPartManager(manager =>
        {
            manager.FeatureProviders.Add(new PluginControllerFeatureProvider());
        });
        
        // Add controller route convention
        builder.AddMvcOptions(options =>
        {
            options.Conventions.Add(new PluginControllerRouteConvention());
        });
        
        return builder;
    }
}
