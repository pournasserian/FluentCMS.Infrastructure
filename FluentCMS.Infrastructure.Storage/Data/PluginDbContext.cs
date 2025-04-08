using FluentCMS.Infrastructure.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace FluentCMS.Infrastructure.Storage.Data
{
    public class PluginDbContext : DbContext
    {
        public PluginDbContext(DbContextOptions<PluginDbContext> options) 
            : base(options)
        {
        }

        public DbSet<PluginMetadata> Plugins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure plugin metadata entity
            modelBuilder.Entity<PluginMetadata>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Version).IsRequired();
                entity.Property(p => p.AssemblyPath).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
