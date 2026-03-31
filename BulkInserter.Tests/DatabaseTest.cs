using Xunit;

namespace BulkInserter.Tests;

[Collection(DatabaseCollection.Name)]
public abstract class DatabaseTest(DatabaseFixture fixture)
{
    protected TestDbContext Db { get; } = fixture.Db;
}
