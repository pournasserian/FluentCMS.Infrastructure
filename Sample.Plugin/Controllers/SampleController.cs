using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sample.Plugin.Controllers
{
    // Simple handler for plugin endpoints
    public class SampleEndpointHandler
    {
        private readonly ISampleService _sampleService;
        private readonly ILogger<SampleEndpointHandler> _logger;

        public SampleEndpointHandler(ISampleService sampleService, ILogger<SampleEndpointHandler> logger)
        {
            _sampleService = sampleService;
            _logger = logger;
        }

        public async Task HandleGetMessage(HttpContext context)
        {
            _logger.LogInformation("Sample endpoint handler GetMessage called");
            
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(_sampleService.GetMessage());
        }

        public async Task HandleGetInfo(HttpContext context)
        {
            _logger.LogInformation("Sample endpoint handler GetInfo called");
            
            var info = new Dictionary<string, string>
            {
                { "name", "Sample Plugin" },
                { "version", "1.0.0" },
                { "description", "A sample plugin that demonstrates the plugin system capabilities" }
            };
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(info));
        }

        public async Task HandleEcho(HttpContext context)
        {
            _logger.LogInformation("Sample endpoint handler Echo called");
            
            // Simple echo handler
            using var reader = new System.IO.StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            
            var response = new 
            {
                Message = $"Echo: {body}",
                Timestamp = DateTime.UtcNow
            };
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
