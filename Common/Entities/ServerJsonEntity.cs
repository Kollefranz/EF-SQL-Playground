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

public record DiskJson
{
    public Guid Id { get; init; }
    public required string MountPoint { get; init; }
    public required string DiskType { get; init; }
    public long CapacityGb { get; init; }
    public long UsedGb { get; init; }
}

public record NetworkInterfaceJson
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string MacAddress { get; init; }
    public string? IpAddress { get; init; }
    public string? SubnetMask { get; init; }
    public int? VlanId { get; init; }
    public bool IsEnabled { get; init; }
}

public record InstalledServiceJson
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public int Port { get; init; }
    public required string Status { get; init; }
    public DateTime InstalledAt { get; init; }
}

public record ServerTagJson
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
}
