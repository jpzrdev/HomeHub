using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeHub.Infrastructure.Data.EntityConfigurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
       public void Configure(EntityTypeBuilder<InventoryItem> builder)
       {
              builder.HasKey(p => p.Id);
              builder.Property(p => p.Name)
                     .IsRequired()
                     .HasMaxLength(200);

              builder.Property(i => i.MinimumQuantity)
                   .HasPrecision(18, 4);

              builder.Property(i => i.QuantityAvailable)
                     .HasPrecision(18, 4);

              builder.Property(i => i.IsActive)
                     .IsRequired();
       }
}