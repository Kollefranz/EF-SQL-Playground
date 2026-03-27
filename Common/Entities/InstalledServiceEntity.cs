using System.Text.Json.Serialization;

namespace Common.Entities;

public record InstalledServiceEntity
{
    public Guid Id { get; init; }
    public int ServerId { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required int Port { get; init; }
    public required string Status { get; init; }
    public DateTime InstalledAt { get; init; }

    [JsonIgnore]
    public ServerEntity? Server { get; init; }
}
