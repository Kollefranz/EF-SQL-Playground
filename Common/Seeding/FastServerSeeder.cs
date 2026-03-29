using Common.Entities;

namespace Common.Seeding;

public static class FastServerSeeder
{
    static readonly string[] Statuses = ["active", "inactive", "decommissioned"];
    static readonly string[] Environments = ["production", "staging", "development"];
    static readonly string[] OperatingSystems =
    [
        "Ubuntu 24.04",
        "Debian 12",
        "RHEL 9",
        "Windows Server 2022",
    ];
    static readonly int[] CpuOptions = [2, 4, 8, 16, 32];
    static readonly int[] MemoryOptions = [2048, 4096, 8192, 16384, 32768];
    static readonly long[] DiskSizes = [256L, 512L, 1024L, 2048L, 4096L];
    static readonly string[] DiskTypes = ["SSD", "HDD", "NVMe"];
    static readonly string[] NicNames = ["eth0", "eth1", "ens3", "bond0"];
    static readonly string[] ServiceNames =
    [
        "nginx",
        "postgresql",
        "redis",
        "docker",
        "prometheus",
        "grafana",
    ];
    static readonly string[] ServiceStatuses = ["running", "stopped", "failed"];
    static readonly string[] TagKeys = ["team", "app", "tier", "region", "cost-center"];
    static readonly string[] TagValues =
    [
        "alpha",
        "beta",
        "gamma",
        "delta",
        "epsilon",
        "zeta",
        "eta",
        "theta",
    ];

    public static ServerJsonEntity[] Generate(long count)
    {
        var results = new ServerJsonEntity[count];
        Parallel.For(0, count, i => results[i] = GenerateOne(i));
        return results;
    }

    static ServerJsonEntity GenerateOne(long index)
    {
        var r = Random.Shared;
        var status = Pick(r, Statuses);
        var provisionedAt = DateTime.UtcNow.AddDays(-r.Next(1, 365 * 5));

        return new ServerJsonEntity
        {
            Id = Guid.NewGuid(),
            Hostname = $"srv-{index:x6}-{r.Next(0, 0xffff):x4}",
            IpAddress = Ip(r),
            OperatingSystem = Pick(r, OperatingSystems),
            CpuCores = Pick(r, CpuOptions),
            MemoryMb = Pick(r, MemoryOptions),
            Status = status,
            Environment = Pick(r, Environments),
            ProvisionedAt = provisionedAt,
            DecommissionedAt =
                status == "decommissioned"
                    ? provisionedAt.AddDays(
                        r.Next(1, (int)(DateTime.UtcNow - provisionedAt).TotalDays)
                    )
                    : null,
            Disks = GenerateDisks(r),
            NetworkInterfaces = GenerateNics(r),
            InstalledServices = GenerateServices(r),
            Tags = GenerateTags(r),
        };
    }

    static List<DiskJson> GenerateDisks(Random r)
    {
        var count = r.Next(1, 5);
        var list = new List<DiskJson>(count);
        for (var i = 0; i < count; i++)
        {
            var capacity = Pick(r, DiskSizes);
            list.Add(
                new DiskJson
                {
                    Id = Guid.NewGuid(),
                    MountPoint = i == 0 ? "/" : $"/data{i}",
                    DiskType = Pick(r, DiskTypes),
                    CapacityGb = capacity,
                    UsedGb = r.NextInt64(0, capacity),
                }
            );
        }
        return list;
    }

    static List<NetworkInterfaceJson> GenerateNics(Random r)
    {
        var count = r.Next(1, 4);
        var list = new List<NetworkInterfaceJson>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(
                new NetworkInterfaceJson
                {
                    Id = Guid.NewGuid(),
                    Name = Pick(r, NicNames),
                    MacAddress = Mac(r),
                    IpAddress = Ip(r),
                    SubnetMask = "255.255.255.0",
                    VlanId = r.Next(0, 10) < 3 ? r.Next(1, 4095) : null,
                    IsEnabled = r.Next(0, 10) > 0,
                }
            );
        }
        return list;
    }

    static List<InstalledServiceJson> GenerateServices(Random r)
    {
        var count = r.Next(0, 6);
        var list = new List<InstalledServiceJson>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(
                new InstalledServiceJson
                {
                    Id = Guid.NewGuid(),
                    Name = Pick(r, ServiceNames),
                    Version = $"{r.Next(1, 6)}.{r.Next(0, 21)}.{r.Next(0, 11)}",
                    Port = r.Next(1024, 65536),
                    Status = Pick(r, ServiceStatuses),
                    InstalledAt = DateTime.UtcNow.AddDays(-r.Next(1, 365 * 2)),
                }
            );
        }
        return list;
    }

    static List<ServerTagJson> GenerateTags(Random r)
    {
        var count = r.Next(1, 6);
        var list = new List<ServerTagJson>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(
                new ServerTagJson
                {
                    Id = Guid.NewGuid(),
                    Key = Pick(r, TagKeys),
                    Value = Pick(r, TagValues),
                }
            );
        }
        return list;
    }

    static string Ip(Random r) =>
        $"{r.Next(1, 255)}.{r.Next(0, 256)}.{r.Next(0, 256)}.{r.Next(1, 255)}";

    static string Mac(Random r) =>
        $"{r.Next(0, 256):x2}:{r.Next(0, 256):x2}:{r.Next(0, 256):x2}:{r.Next(0, 256):x2}:{r.Next(0, 256):x2}:{r.Next(0, 256):x2}";

    static T Pick<T>(Random r, T[] arr) => arr[r.Next(arr.Length)];
}
