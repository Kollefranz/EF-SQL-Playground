using Common;
using Microsoft.EntityFrameworkCore;

namespace API;

public interface IQueryLensDbContextFactory<out TContext>
    where TContext : DbContext
{
    TContext CreateOfflineContext();
}

public sealed class QueryLensFactory : IQueryLensDbContextFactory<TheApiDbContext>
{
    public TheApiDbContext CreateOfflineContext()
    {
        // Dummy setup. Only needed for determining SQL dialect (e.g. PostgreSQL, SQL Server)
        // No real connection string is needed
        var options = new DbContextOptionsBuilder<TheApiDbContext>()
            .UseSqlServer(
                string.Empty
            )
            .Options;

        return new TheApiDbContext(options);
    }
}
