using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechTales.Data.Models;

namespace TechTales.Data.Configurations;

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTagEntity>
{
    public void Configure(EntityTypeBuilder<BlogTagEntity> builder)
    {
        builder.HasKey(bt => new { bt.BlogId, bt.TagId });
    }
}