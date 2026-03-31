namespace Common.Entities;

public record ServerTagJson
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
}