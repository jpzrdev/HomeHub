using HomeHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeHub.Infrastructure.Data.EntityConfigurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.HasKey(rs => rs.Id);

        builder.HasOne(rs => rs.Recipe)
            .WithMany(r => r.Steps)
            .HasForeignKey(rs => rs.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(rs => rs.Order)
            .IsRequired();

        builder.Property(rs => rs.Description)
            .IsRequired()
            .HasMaxLength(1000);

        // Ensure unique order per recipe
        builder.HasIndex(rs => new { rs.RecipeId, rs.Order })
            .IsUnique();
    }
}

