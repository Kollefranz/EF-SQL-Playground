namespace Common.Entities;

public record ServerJsonEntity
{
    public int RowId { get; init; }
    public Guid Id { get; init; }
    public required string Hostname { get; init; }
    public required string IpAddress { get; init; }
    public required string OperatingSystem { get; init; }
    public int CpuCores { get; init; }
    public int MemoryMb { get; init; }
    public required string Status { get; init; }
    public required string Environment { get; init; }
    public DateTime ProvisionedAt { get; init; }
    public DateTime? DecommissionedAt { get; init; }
    public IList<DiskJson> Disks { get; init; } = [];
    public IList<NetworkInterfaceJson> NetworkInterfaces { get; init; } = [];
    public IList<InstalledServiceJson> InstalledServices { get; init; } = [];
    public IList<ServerTagJson> Tags { get; init; } = [];
}