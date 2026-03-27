using Bogus;
using Common.Entities;

namespace Common.Seeding;

public static class ServerSeeder
{
    public static IEnumerable<ServerJsonEntity> Generate(int count)
    {
        for (var i = 0; i < count; i++)
            yield return GenerateOne();
    }

    private static ServerJsonEntity GenerateOne()
    {
        var f = new Faker();

        var disks = new Faker<DiskJson>()
            .RuleFor(d => d.Id, _ => Guid.NewGuid())
            .RuleFor(d => d.MountPoint, x => x.System.DirectoryPath())
            .RuleFor(d => d.DiskType, x => x.PickRandom("SSD", "HDD", "NVMe"))
            .RuleFor(d => d.CapacityGb, x => x.PickRandom(256L, 512L, 1024L, 2048L, 4096L))
            .RuleFor(d => d.UsedGb, (x, d) => x.Random.Long(0, d.CapacityGb))
            .Generate(f.Random.Int(1, 4));

        var nics = new Faker<NetworkInterfaceJson>()
            .RuleFor(n => n.Id, _ => Guid.NewGuid())
            .RuleFor(n => n.Name, x => x.PickRandom("eth0", "eth1", "ens3", "bond0"))
            .RuleFor(n => n.MacAddress, x => x.Internet.Mac())
            .RuleFor(n => n.IpAddress, x => x.Internet.Ip())
            .RuleFor(n => n.SubnetMask, _ => "255.255.255.0")
            .RuleFor(n => n.VlanId, x => x.Random.Bool(0.3f) ? x.Random.Int(1, 4094) : null)
            .RuleFor(n => n.IsEnabled, x => x.Random.Bool(0.9f))
            .Generate(f.Random.Int(1, 3));

        var services = new Faker<InstalledServiceJson>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Name, x => x.PickRandom("nginx", "postgresql", "redis", "docker", "prometheus", "grafana"))
            .RuleFor(s => s.Version, x => $"{x.Random.Int(1, 5)}.{x.Random.Int(0, 20)}.{x.Random.Int(0, 10)}")
            .RuleFor(s => s.Port, x => x.Internet.Port())
            .RuleFor(s => s.Status, x => x.PickRandom("running", "stopped", "failed"))
            .RuleFor(s => s.InstalledAt, x => x.Date.Past(2))
            .Generate(f.Random.Int(0, 5));

        var tags = new Faker<ServerTagJson>()
            .RuleFor(t => t.Id, _ => Guid.NewGuid())
            .RuleFor(t => t.Key, x => x.PickRandom("team", "app", "tier", "region", "cost-center"))
            .RuleFor(t => t.Value, x => x.Lorem.Word())
            .Generate(f.Random.Int(1, 5));

        var provisionedAt = f.Date.Past(5);
        var status = f.PickRandom("active", "inactive", "decommissioned");

        return new Faker<ServerJsonEntity>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Hostname, x => x.Internet.DomainWord() + "-" + x.Random.AlphaNumeric(4))
            .RuleFor(s => s.IpAddress, x => x.Internet.Ip())
            .RuleFor(s => s.OperatingSystem, x => x.PickRandom("Ubuntu 24.04", "Debian 12", "RHEL 9", "Windows Server 2022"))
            .RuleFor(s => s.CpuCores, x => x.PickRandom(2, 4, 8, 16, 32))
            .RuleFor(s => s.MemoryMb, x => x.PickRandom(2048, 4096, 8192, 16384, 32768))
            .RuleFor(s => s.Status, _ => status)
            .RuleFor(s => s.Environment, x => x.PickRandom("production", "staging", "development"))
            .RuleFor(s => s.ProvisionedAt, _ => provisionedAt)
            .RuleFor(s => s.DecommissionedAt, x => status == "decommissioned" ? x.Date.Between(provisionedAt, DateTime.UtcNow) : null)
            .RuleFor(s => s.Disks, _ => disks)
            .RuleFor(s => s.NetworkInterfaces, _ => nics)
            .RuleFor(s => s.InstalledServices, _ => services)
            .RuleFor(s => s.Tags, _ => tags)
            .Generate();
    }
}
