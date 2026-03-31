using System.Data;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace BulkInserter.Tests;

[TestSubject(typeof(BulkSyncer))]
public class BulkSyncerTest(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public void TestBuildDataTable_WithCommonClrTypes()
    {
        var dataTable = BulkSyncer.BuildDataTable<TestEntity>(Db);

        dataTable.Columns.Count.ShouldBe(7);

        dataTable.Columns.Contains("Id").ShouldBeTrue();
        dataTable.Columns.Contains("Name").ShouldBeTrue();
        dataTable.Columns.Contains("CreatedAt").ShouldBeTrue();
        dataTable.Columns.Contains("UpdatedAt").ShouldBeTrue();
        dataTable.Columns.Contains("Duration").ShouldBeTrue();
        dataTable.Columns.Contains("Count").ShouldBeTrue();
        dataTable.Columns.Contains("LargeCount").ShouldBeTrue();
        dataTable.Columns.Contains("VeryLargeCount").ShouldBeTrue();
    }
}
