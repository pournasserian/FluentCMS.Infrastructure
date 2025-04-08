using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.Plugin
{
    // Sample background service that demonstrates plugin background task capabilities
    public class SampleBackgroundService : BackgroundService
    {
        private readonly ILogger<SampleBackgroundService> _logger;
        private readonly TimeSpan _processInterval = TimeSpan.FromSeconds(30);

        public SampleBackgroundService(ILogger<SampleBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sample background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Sample background service is processing at: {Time}", DateTime.UtcNow);
                    
                    // Simulate some work
                    await Task.Delay(500, stoppingToken);
                    
                    // Wait for next processing interval
                    await Task.Delay(_processInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, just exit
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in sample background service");
                    
                    // Wait a bit before retrying after an error
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            
            _logger.LogInformation("Sample background service stopped");
        }
    }
}
