using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCMS.Infrastructure.Host.Middleware
{
    // A wrapper around IApplicationBuilder that provides plugin context
    public class PluginApplicationBuilder : IApplicationBuilder
    {
        private const string PluginIdKey = "PluginId";
        private readonly IApplicationBuilder _innerBuilder;
        private readonly string _pluginId;
        
        public PluginApplicationBuilder(IApplicationBuilder app, string pluginId)
        {
            _innerBuilder = app;
            _pluginId = pluginId;
        }

        public IServiceProvider ApplicationServices 
        { 
            get => _innerBuilder.ApplicationServices; 
            set => _innerBuilder.ApplicationServices = value; 
        }
        
        public IDictionary<string, object> Properties => _innerBuilder.Properties;
        
        public IFeatureCollection ServerFeatures => _innerBuilder.ServerFeatures;
        
        public RequestDelegate Build() => _innerBuilder.Build();
        
        public IApplicationBuilder New()
        {
            return new PluginApplicationBuilder(_innerBuilder.New(), _pluginId);
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _innerBuilder.Use(next =>
            {
                var wrappedMiddleware = middleware(next);
                
                return async context =>
                {
                    // Add plugin context to the HttpContext
                    context.Items[PluginIdKey] = _pluginId;
                    
                    // Execute the middleware
                    await wrappedMiddleware(context);
                };
            });
            
            return this;
        }
        
        // Helper method to get plugin ID from HttpContext
        public static string GetPluginId(HttpContext context)
        {
            if (context.Items.TryGetValue(PluginIdKey, out var pluginId))
            {
                return pluginId as string;
            }
            
            return null;
        }
    }
}
