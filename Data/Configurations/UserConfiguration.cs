using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TechTales.Data.Models;

namespace TechTales.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder
            .Property(u => u.UserName)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .Property(u => u.Email)
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder
            .HasMany(u => u.Blogs)
            .WithOne(b => b.Author);

        builder
            .Property(u => u.Name)
            .HasColumnType("varchar(50)");

        builder
            .Property(u => u.Surname)
            .HasColumnType("varchar(50)");

        builder
            .Property(u => u.AboutMe)
            .HasColumnType("longtext");

        builder
            .Property(u => u.Country)
            .HasColumnType("varchar(60)");

        builder
            .Property(u => u.Avatar)
            .HasColumnType("longblob");
    }
}
