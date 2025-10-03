using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.Infrastructure.Data;

public class HomeHubContext : DbContext
{
    public HomeHubContext(DbContextOptions<HomeHubContext> options)
            : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeHubContext).Assembly);
    }
}