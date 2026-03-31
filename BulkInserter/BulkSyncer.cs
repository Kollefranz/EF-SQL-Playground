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
    /// Only maps properties with a corresponding CLR property (shadow properties are skipped).
    public static DataTable BuildDataTable<T>(IEnumerable<T> entities, IEntityType entityType)
    {
        var tableName = entityType.GetTableName() ?? typeof(T).Name;
        var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());

        var columns = entityType.GetProperties()
            .Select(p => (Property: p, ColumnName: p.GetColumnName(storeObject)))
            .Where(x => x.ColumnName is not null && x.Property.PropertyInfo is not null)
            .Select(x => (x.Property, ColumnName: x.ColumnName!))
            .ToArray();

        var dt = new DataTable(tableName);
        foreach (var (prop, columnName) in columns)
        {
            var clrType = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
            dt.Columns.Add(columnName, clrType);
        }

        foreach (var entity in entities)
        {
            var row = dt.NewRow();
            foreach (var (prop, columnName) in columns)
            {
                row[columnName] = prop.PropertyInfo!.GetValue(entity) ?? DBNull.Value;
            }
            dt.Rows.Add(row);
        }

        return dt;
    }

    /// Returns entity types sorted so principals come before dependents — safe for FK-constrained inserts.
    /// Ownership FKs are excluded; cycles (self-referencing FKs) are appended at the end.
    public static IReadOnlyList<IEntityType> GetInsertOrder(IEnumerable<IEntityType> entityTypes) =>
        TopologicalSort(
            entityTypes,
            t => t.GetForeignKeys()
                  .Where(fk => !fk.IsOwnership)
                  .Select(fk => fk.PrincipalEntityType)
                  .Where(p => !p.Equals(t)));

    /// Convenience overload: returns all entity types in the model in safe insert order.
    public static IReadOnlyList<IEntityType> GetInsertOrder(IModel model) =>
        GetInsertOrder(model.GetEntityTypes());

    /// Topological sort using Kahn's algorithm. Nodes involved in cycles are appended after sorted nodes.
    /// Kahn's algorithm is a linear time algorithm for topological sorting of a directed acyclic graph.
    /// O(|V| + |E|) where V is the number of nodes and E is the number of edges.
    public static IReadOnlyList<T> TopologicalSort<T>(
        IEnumerable<T> nodes,
        Func<T, IEnumerable<T>> getDependencies) where T : notnull
    {
        var nodeList = nodes.ToList();
        var nodeSet = nodeList.ToHashSet();

        var dependents = nodeList.ToDictionary(n => n, _ => new List<T>());
        var inDegree = nodeList.ToDictionary(n => n, _ => 0);

        foreach (var node in nodeList)
        {
            foreach (var dep in getDependencies(node).Where(nodeSet.Contains))
            {
                dependents[dep].Add(node);
                inDegree[node]++;
            }
        }

        var queue = new Queue<T>(nodeList.Where(n => inDegree[n] == 0));
        var result = new List<T>(nodeList.Count);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var dependent in dependents[current])
            {
                if (--inDegree[dependent] == 0)
                    queue.Enqueue(dependent);
            }
        }

        // Append nodes involved in cycles (e.g. self-referencing nullable FKs)
        var visited = result.ToHashSet();
        foreach (var node in nodeList)
        {
            if (!visited.Contains(node))
                result.Add(node);
        }

        return result;
    }
}
