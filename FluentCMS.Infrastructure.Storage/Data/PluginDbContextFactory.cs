using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FluentCMS.Infrastructure.Storage.Data
{
    // Factory for creating DbContext instances during design time (migrations)
    public class PluginDbContextFactory : IDesignTimeDbContextFactory<PluginDbContext>
    {
        public PluginDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=FluentCmsPlugins;Trusted_Connection=True;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<PluginDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PluginDbContext(optionsBuilder.Options);
        }
    }
}
