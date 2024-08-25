namespace TechTales.Helpers;

public static class EntityParser
{
    public static async Task<List<TEntity>> ParseAsync<TEntity>(
        string? input, 
        char[] separator, 
        Func<string, Task<TEntity?>> entityRetriever, 
        Func<string, TEntity> entityCreator
    ) where TEntity : class
    {
        var entityNames = input?
            .Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var entities = new List<TEntity>();

        if (entityNames is not null)
        {
            foreach (var name in entityNames)
            {
                var entity = await entityRetriever(name) ?? entityCreator(name);
                entities.Add(entity);
            }
        }

        return entities;
    }
}