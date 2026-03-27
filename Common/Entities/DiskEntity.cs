using System.Text.Json.Serialization;

namespace Common.Entities;

public record DiskEntity
{
    public Guid Id { get; init; }
    public int ServerId { get; init; }
    public required string MountPoint { get; init; }
    public required long CapacityGb { get; init; }
    public required string DiskType { get; init; }
    public long UsedGb { get; init; }

    [JsonIgnore]
    public ServerEntity? Server { get; init; }
}
