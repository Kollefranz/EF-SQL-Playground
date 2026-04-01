using System;
using System.Data;
using System.Linq;
using System.Reflection;
using AutoFixture.Xunit3;
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
        var props = typeof(TestEntity).GetProperties();
        dataTable.Columns.Count.ShouldBe(props.Length);
        foreach (var propertyInfo in props)
        {
            dataTable.Columns.Contains(propertyInfo.Name).ShouldBeTrue();
        }
    }


    [Theory]
    [InlineAutoData]
    public void DataTableInsert_ActualValuesFromEntities(TestEntity[] entities)
    {
        var dataTable = BulkSyncer.BuildDataTable<TestEntity>(Db);

        var props = typeof(TestEntity).GetProperties();
        foreach (var entity in entities)
        {
            var row = dataTable.NewRow();
            
            foreach (PropertyInfo prop in props)
            {
                var value = prop.GetMethod!.Invoke(entity, null);
                
                // todo missing inserts for the row
            }
        }

        dataTable.Rows.Count.ShouldBe(entities.Length);
        foreach (var entity in entities)
        {
            var row = dataTable.Rows.Find(entity.Id);
            row.ShouldNotBeNull();
            
            foreach (PropertyInfo prop in props)
            {
                var columnName = prop.Name;
                row[columnName].ShouldBe(prop.GetValue(entity));
            }
        }
    }
}