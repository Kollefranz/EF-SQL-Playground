namespace API.Entities;

public record DiskEntity
{
    public Guid Id { get; init; }
    public required Guid ServerId { get; init; }
    public required string MountPoint { get; init; }
    public required long CapacityGb { get; init; }
    public required string DiskType { get; init; }
    public long UsedGb { get; init; }

    public ServerEntity? Server { get; init; }
}