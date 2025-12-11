using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeHub.Infrastructure.Data.EntityConfigurations;

public class ShoppingListConfiguration : IEntityTypeConfiguration<ShoppingList>
{
    public void Configure(EntityTypeBuilder<ShoppingList> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.HasMany(sl => sl.Items)
            .WithOne(sli => sli.ShoppingList)
            .HasForeignKey(sli => sli.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(sl => sl.IsCompleted)
            .IsRequired();
    }
}
