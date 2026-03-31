using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BulkInserter.Tests;

public class DatabaseFixture
{
    public TestDbContext Db { get; }

    public DatabaseFixture()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<DatabaseFixture>()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection not found in user secrets."
            );

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        Db = new TestDbContext(options);
    }
}
