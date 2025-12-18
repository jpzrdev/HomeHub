using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeHub.Infrastructure.Data.EntityConfigurations;

public class ShoppingListItemConfiguration : IEntityTypeConfiguration<ShoppingListItem>
{
    public void Configure(EntityTypeBuilder<ShoppingListItem> builder)
    {
        builder.HasKey(sli => sli.Id);

        builder.HasOne(sli => sli.ShoppingList)
            .WithMany(sl => sl.Items)
            .HasForeignKey(sli => sli.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sli => sli.InventoryItem)
            .WithMany()
            .HasForeignKey(sli => sli.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sli => sli.QuantityToBuy)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(sli => sli.IsPurchased)
            .IsRequired();

        builder.Property(sli => sli.IsActive)
            .IsRequired();
    }
}
