namespace Common.Entities;

public record InstalledServiceJson
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public int Port { get; init; }
    public required string Status { get; init; }
    public DateTime InstalledAt { get; init; }
}