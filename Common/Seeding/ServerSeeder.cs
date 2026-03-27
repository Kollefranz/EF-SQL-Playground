using Bogus;
using Common.Entities;

namespace Common.Seeding;

public static class ServerSeeder
{
    public static List<ServerEntity> Generate(int count)
    {
        var diskFaker = new Faker<DiskEntity>()
            .RuleFor(d => d.Id, _ => Guid.NewGuid())
            .RuleFor(d => d.MountPoint, f => f.System.DirectoryPath())
            .RuleFor(d => d.DiskType, f => f.PickRandom("SSD", "HDD", "NVMe"))
            .RuleFor(d => d.CapacityGb, f => f.PickRandom(256L, 512L, 1024L, 2048L, 4096L))
            .RuleFor(d => d.UsedGb, (f, d) => f.Random.Long(0, d.CapacityGb));

        var nicFaker = new Faker<NetworkInterfaceEntity>()
            .RuleFor(n => n.Id, _ => Guid.NewGuid())
            .RuleFor(n => n.Name, f => f.PickRandom("eth0", "eth1", "ens3", "bond0"))
            .RuleFor(n => n.MacAddress, f => f.Internet.Mac())
            .RuleFor(n => n.IpAddress, f => f.Internet.Ip())
            .RuleFor(n => n.SubnetMask, _ => "255.255.255.0")
            .RuleFor(n => n.VlanId, f => f.Random.Bool(0.3f) ? f.Random.Int(1, 4094) : null)
            .RuleFor(n => n.IsEnabled, f => f.Random.Bool(0.9f));

        var serviceFaker = new Faker<InstalledServiceEntity>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Name, f => f.PickRandom("nginx", "postgresql", "redis", "docker", "prometheus", "grafana"))
            .RuleFor(s => s.Version, f => $"{f.Random.Int(1, 5)}.{f.Random.Int(0, 20)}.{f.Random.Int(0, 10)}")
            .RuleFor(s => s.Port, f => f.Internet.Port())
            .RuleFor(s => s.Status, f => f.PickRandom("running", "stopped", "failed"))
            .RuleFor(s => s.InstalledAt, f => f.Date.Past(2));

        var tagFaker = new Faker<ServerTagEntity>()
            .RuleFor(t => t.Id, _ => Guid.NewGuid())
            .RuleFor(t => t.Key, f => f.PickRandom("team", "app", "tier", "region", "cost-center"))
            .RuleFor(t => t.Value, f => f.Lorem.Word());

        var serverFaker = new Faker<ServerEntity>()
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.Hostname, f => f.Internet.DomainWord() + "-" + f.Random.AlphaNumeric(4))
            .RuleFor(s => s.IpAddress, f => f.Internet.Ip())
            .RuleFor(s => s.OperatingSystem, f => f.PickRandom("Ubuntu 24.04", "Debian 12", "RHEL 9", "Windows Server 2022"))
            .RuleFor(s => s.CpuCores, f => f.PickRandom(2, 4, 8, 16, 32))
            .RuleFor(s => s.MemoryMb, f => f.PickRandom(2048, 4096, 8192, 16384, 32768))
            .RuleFor(s => s.Status, f => f.PickRandom("active", "inactive", "decommissioned"))
            .RuleFor(s => s.Environment, f => f.PickRandom("production", "staging", "development"))
            .RuleFor(s => s.ProvisionedAt, f => f.Date.Past(5))
            .RuleFor(s => s.DecommissionedAt, (f, s) => s.Status == "decommissioned" ? f.Date.Between(s.ProvisionedAt, DateTime.UtcNow) : null)
            .RuleFor(s => s.Disks, (f, s) => diskFaker.Clone().RuleFor(d => d.ServerId, _ => s.Id).Generate(f.Random.Int(1, 4)))
            .RuleFor(s => s.NetworkInterfaces, (f, s) => nicFaker.Clone().RuleFor(n => n.ServerId, _ => s.Id).Generate(f.Random.Int(1, 3)))
            .RuleFor(s => s.InstalledServices, (f, s) => serviceFaker.Clone().RuleFor(sv => sv.ServerId, _ => s.Id).Generate(f.Random.Int(0, 5)))
            .RuleFor(s => s.Tags, (f, s) => tagFaker.Clone().RuleFor(t => t.ServerId, _ => s.Id).Generate(f.Random.Int(1, 5)));

        return serverFaker.Generate(count);
    }
}
