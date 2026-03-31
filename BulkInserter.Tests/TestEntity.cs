using System;

namespace BulkInserter.Tests;

public record TestEntity
{
    public required Guid Id { get; set; } 
    public required DateTime CreatedAt { get; set; } 
    public required DateTimeOffset UpdatedAt { get; set; } 
    public required TimeSpan Duration { get; set; } 
    public required int Count { get; set; } 
    public required long LargeCount { get; set; } 
    public required ulong VeryLargeCount { get; set; }
}