using FinTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTracker.Infrastructure.Data.Configurations;

public class CategoryTagConfiguration : IEntityTypeConfiguration<CategoryTag>
{
    public void Configure(EntityTypeBuilder<CategoryTag> builder)
    {
        builder.HasKey(ct => new { ct.CategoryId, ct.TagId });

        builder.HasOne(ct => ct.Category)
            .WithMany(c => c.CategoryTags)
            .HasForeignKey(ct => ct.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.Tag)
            .WithMany(t => t.CategoryTags)
            .HasForeignKey(ct => ct.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
