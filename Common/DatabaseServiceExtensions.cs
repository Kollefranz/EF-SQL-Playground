using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddTheApiDatabase(
        this IServiceCollection services,
        string? connectionString
    )
    {
        services.AddDbContext<TheApiDbContext>(options =>
            options.UseSqlServer(connectionString)
        );
        return services;
    }
}
