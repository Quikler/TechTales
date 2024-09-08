using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TechTales.Data.Models;
using TechTales.Models;

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

    public static async Task<List<CategoryEntity>> ParseAndAddNewAsync(this DbSet<CategoryEntity> categories, string? input)
    {
        if (input is null) return new List<CategoryEntity>();

        var inputCategories = input.Split(new char[] { ' ', '#', ',', '.', '|' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var existingCategories = await categories
            .Where(t => inputCategories.Contains(t.Name))
            .ToListAsync();

        var newTags = inputCategories
            .Except(existingCategories.Select(t => t.Name))  // Only take categories that are not in the database
            .Select(name => new CategoryEntity { Name = name })  // Create new CategoryEntity objects
            .ToList();

        // Combine existing and new categories
        var allTags = existingCategories.Concat(newTags).ToList();

        return allTags;
    }

    public static async Task<List<TagEntity>> ParseAndAddNewAsync(this DbSet<TagEntity> tags, string? input)
    {
        if (input is null) return new List<TagEntity>();

        var inputTags = input.Split(new char[] { ' ', '#', ',', '.', '|' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var existingTags = await tags
            .Where(t => inputTags.Contains(t.Name))
            .ToListAsync();

        var newTags = inputTags
            .Except(existingTags.Select(t => t.Name))  // Only take tags that are not in the database
            .Select(name => new TagEntity { Name = name })  // Create new TagEntity objects
            .ToList();

        // Combine existing and new tags
        var allTags = existingTags.Concat(newTags).ToList();

        return allTags;
    }

    public static async Task<PaginationViewModel<TOut>> GetPaginationAsync<TIn, TOut>(this DbSet<TIn> dbSet, Expression<Func<TIn, bool>> predicate, int pageSize)
        where TIn : class
        where TOut : class
    {
        var total = await dbSet.CountAsync(predicate);
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        // var total = await _context.Categories.CountAsync(t => t.Blogs.Count > 0);
        // var totalPages = (int)Math.Ceiling((double)total / pageSize);

        // var model = new PaginationViewModel<CategoryViewModel>(page, totalPages);

        return new PaginationViewModel<TOut>(totalPages);
    }
}