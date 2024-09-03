using Microsoft.EntityFrameworkCore;
using TechTales.Data.Models;

namespace TechTales.Helpers.Extensions;

public static class DbSetExtensions
{
    public static async Task RemoveUnusedAsync(this DbSet<TagEntity> tags)
    {
        var unusedTags = await tags
            .Where(t => t.Blogs.Count <= 0)
            .ToArrayAsync();

        if (unusedTags.Length > 0)
        {
            tags.RemoveRange(unusedTags);
        }
    }

    public static async Task RemoveUnusedAsync(this DbSet<CategoryEntity> categories)
    {
        var unusedTags = await categories
            .Where(t => t.Blogs.Count <= 0)
            .ToArrayAsync();

        if (unusedTags.Length > 0)
        {
            categories.RemoveRange(unusedTags);
        }
    }

    public static async Task<List<CategoryEntity>> ParseAsync(this DbSet<CategoryEntity> categories, string? input)
    {
        if (input is null) return [];

        var entityNames = input.Split(new char[] { ' ', '#', ',', '.', '|' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var existingEntities = await categories
            .Where(c => entityNames.Contains(c.Name))
            .ToListAsync();

        var entities = new List<CategoryEntity>();

        entities.AddRange(existingEntities);

        // Add new categories if they are not in the database
        foreach (var name in entityNames)
        {
            if (!existingEntities.Any(e => e.Name == name))
            {
                entities.Add(new CategoryEntity { Name = name });
            }
        }

        return entities;
    }

    public static async Task<List<TagEntity>> ParseAsync(this DbSet<TagEntity> tags, string? input)
    {
        if (input is null) return [];

        var entityNames = input.Split(new char[] { ' ', '#', ',', '.', '|' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var existingEntities = await tags
            .Where(t => entityNames.Contains(t.Name))
            .ToListAsync();

        var entities = new List<TagEntity>();

        entities.AddRange(existingEntities);

        // Add new tags if they are not in the database
        foreach (var name in entityNames)
        {
            if (!existingEntities.Any(e => e.Name == name))
            {
                entities.Add(new TagEntity { Name = name });
            }
        }

        return entities;
    }
}