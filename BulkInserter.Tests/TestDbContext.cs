using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BulkInserter.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext() { }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; }

    // Used by dotnet-ef tooling and parameterless construction (model inspection tests).
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<TestDbContext>()
                .Build();

            var cs = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(cs ?? "Server=.;Database=dummy;");
        }
    }
}
