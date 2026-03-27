namespace Common.Entities;

public record ServerTagEntity
{
    public Guid Id { get; init; }
    public required Guid ServerId { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }

    public ServerEntity? Server { get; init; }
}
