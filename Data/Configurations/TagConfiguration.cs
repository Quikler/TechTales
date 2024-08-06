using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTales.Data.Models;

namespace TechTales.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .Property(t => t.Name)
            .HasColumnType("varchar(40)")
            .IsRequired();

        builder
            .HasMany(t => t.BlogTags)
            .WithOne(bt => bt.Tag)
            .HasForeignKey(bt => bt.TagId);
    }
}
