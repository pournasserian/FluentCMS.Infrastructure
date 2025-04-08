using Microsoft.Extensions.Hosting;

namespace FluentCMS.Infrastructure.Core.Contracts;

// Interface for plugins to provide background tasks
public interface IBackgroundTaskProvider
{
    // Returns a collection of hosted services to be registered with the host
    IEnumerable<IHostedService> GetBackgroundServices();
}
