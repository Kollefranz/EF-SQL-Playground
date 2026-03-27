namespace API.Entities;

public record InstalledServiceEntity
{
    public Guid Id { get; init; }
    public required Guid ServerId { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required int Port { get; init; }
    public required string Status { get; init; }
    public DateTime InstalledAt { get; init; }

    public ServerEntity? Server { get; init; }
}