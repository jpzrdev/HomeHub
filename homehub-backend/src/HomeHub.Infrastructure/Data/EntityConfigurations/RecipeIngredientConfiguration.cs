using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeHub.Infrastructure.Data.EntityConfigurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.HasKey(ri => ri.Id);

        builder.HasOne(ri => ri.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.InventoryItem)
            .WithMany()
            .HasForeignKey(ri => ri.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ri => ri.Quantity)
            .HasPrecision(18, 4)
            .IsRequired();

        // Ensure unique inventory item per recipe
        builder.HasIndex(ri => new { ri.RecipeId, ri.InventoryItemId })
            .IsUnique();

        builder.Property(ri => ri.IsActive)
            .IsRequired();
    }
}

