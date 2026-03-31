namespace Common.Entities;

public record DiskJson
{
    public Guid Id { get; init; }
    public required string MountPoint { get; init; }
    public required string DiskType { get; init; }
    public long CapacityGb { get; init; }
    public long UsedGb { get; init; }
}