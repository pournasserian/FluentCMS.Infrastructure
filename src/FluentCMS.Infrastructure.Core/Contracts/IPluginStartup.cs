using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Infrastructure.Core.Contracts
{
    // Interface for plugin service configuration and middleware registration
    public interface IPluginStartup
    {
        // Configure services for this plugin
        void ConfigureServices(IServiceCollection services);
        
        // Configure middleware and other app components
        void Configure(IApplicationBuilder app);
    }
}
