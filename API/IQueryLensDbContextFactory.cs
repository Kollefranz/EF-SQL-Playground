using Microsoft.EntityFrameworkCore;

namespace API;

public interface IQueryLensDbContextFactory<out TContext>
    where TContext : DbContext
{
    TContext CreateOfflineContext();
}