using FluentCMS.Infrastructure.Core.Communication;
using FluentCMS.Infrastructure.Plugins.Communication;
using FluentCMS.Infrastructure.Plugins.Discovery;
using FluentCMS.Infrastructure.Plugins.Loading;
using FluentCMS.Infrastructure.Plugins.Options;
using FluentCMS.Infrastructure.Plugins.Registry;
using FluentCMS.Infrastructure.Storage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Infrastructure.Host.Extensions
{
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

            return services;
        }
    }
}
