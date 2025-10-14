using HomeHub.Domain.Common;
using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data;

public class HomeHubContext : DbContext
{
    public HomeHubContext(DbContextOptions<HomeHubContext> options)
            : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeHubContext).Assembly);
    }
}