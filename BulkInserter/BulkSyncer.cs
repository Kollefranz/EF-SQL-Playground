using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BulkInserter;

public static class BulkSyncer
{
    public static async Task BulkInsertAsync<T>(IEnumerable<T> entities, DbContext context, CancellationToken ct = default)
    {
        var dt = new DataTable();
        var props = typeof(T).GetProperties();
    }

    /// Builds a DataTable from entities using EF Core metadata for column names and CLR types.
    /// Skips navigational properties.
    /// The caller is responsible for manually ensuring that sub-entities are handled appropriately.
    public static DataTable BuildDataTable<TEntity>(DbContext context)
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        if (entityType == null)
        {
            throw new ArgumentException($"Entity type {typeof(TEntity).Name} not found in the model.");
        }

        var dt = new DataTable(entityType.GetTableName());

        var properties = entityType.GetProperties().Where(p => !p.IsShadowProperty()).ToList();
        foreach (var property in properties)
        {
            dt.Columns.Add(property.Name, property.ClrType);
        }

        return dt;
    }
}