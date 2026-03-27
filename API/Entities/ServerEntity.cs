namespace API.Entities;

public record ServerEntity
{
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

    public ICollection<DiskEntity> Disks { get; init; } = [];
    public ICollection<NetworkInterfaceEntity> NetworkInterfaces { get; init; } = [];
    public ICollection<InstalledServiceEntity> InstalledServices { get; init; } = [];
    public ICollection<ServerTagEntity> Tags { get; init; } = [];
}