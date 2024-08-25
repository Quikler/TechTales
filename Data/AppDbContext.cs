using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechTales.Data.Configurations;
using TechTales.Data.Models;

namespace TechTales.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<BlogEntity> Blogs { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<CommentEntity> Comments { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ViewBlogEntity> ViewBlogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new BlogConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        
        modelBuilder.Entity<ViewBlogEntity>().HasKey(vb => vb.Id);

        base.OnModelCreating(modelBuilder);
    }
}