using System.Diagnostics;
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

    // Tick accumulators — one Interlocked.Add per sub-method call.
    // Drained per batch via DrainTimings(); not reset between rows.
    static long _diskTicks;
    static long _nicTicks;
    static long _serviceTicks;
    static long _tagTicks;
    static long _macTicks;

    public readonly record struct SubMethodTimings(
        double DisksSeconds,
        double NicsSeconds,
        double ServicesSeconds,
        double TagsSeconds,
        double MacSeconds
    );

    /// <summary>
    /// Atomically reads and resets all sub-method timing accumulators.
    /// Call once per batch; the returned values cover work since the last drain.
    /// </summary>
    public static SubMethodTimings DrainTimings()
    {
        var f = (double)Stopwatch.Frequency;
        return new SubMethodTimings(
            Interlocked.Exchange(ref _diskTicks, 0) / f,
            Interlocked.Exchange(ref _nicTicks, 0) / f,
            Interlocked.Exchange(ref _serviceTicks, 0) / f,
            Interlocked.Exchange(ref _tagTicks, 0) / f,
            Interlocked.Exchange(ref _macTicks, 0) / f
        );
    }

    public static ServerJsonEntity[] Generate(long count)
    {
        var results = new ServerJsonEntity[count];
        Parallel.For(0, count, i => results[i] = GenerateOne(i));
        return results;
    }

    public static IEnumerable<ServerJsonEntity> GenerateLazy(
        long count,
        CancellationToken ct = default
    )
    {
        for (long i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            yield return GenerateOne(i);
        }
    }

    internal static ServerJsonEntity GenerateOne(long index)
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

    static DiskJson[] GenerateDisks(Random r)
    {
        var ts = Stopwatch.GetTimestamp();
        var count = r.Next(1, 5);
        var arr = new DiskJson[count];
        for (var i = 0; i < count; i++)
        {
            var capacity = Pick(r, DiskSizes);
            arr[i] = new DiskJson
            {
                Id = Guid.NewGuid(),
                MountPoint = i == 0 ? "/" : $"/data{i}",
                DiskType = Pick(r, DiskTypes),
                CapacityGb = capacity,
                UsedGb = r.NextInt64(0, capacity),
            };
        }
        Interlocked.Add(ref _diskTicks, Stopwatch.GetTimestamp() - ts);
        return arr;
    }

    static NetworkInterfaceJson[] GenerateNics(Random r)
    {
        var ts = Stopwatch.GetTimestamp();
        var count = r.Next(1, 4);
        var arr = new NetworkInterfaceJson[count];
        for (var i = 0; i < count; i++)
        {
            arr[i] = new NetworkInterfaceJson
            {
                Id = Guid.NewGuid(),
                Name = Pick(r, NicNames),
                MacAddress = Mac(r),
                IpAddress = Ip(r),
                SubnetMask = "255.255.255.0",
                VlanId = r.Next(0, 10) < 3 ? r.Next(1, 4095) : null,
                IsEnabled = r.Next(0, 10) > 0,
            };
        }
        Interlocked.Add(ref _nicTicks, Stopwatch.GetTimestamp() - ts);
        return arr;
    }

    static InstalledServiceJson[] GenerateServices(Random r)
    {
        var ts = Stopwatch.GetTimestamp();
        var count = r.Next(0, 6);
        var arr = new InstalledServiceJson[count];
        for (var i = 0; i < count; i++)
        {
            arr[i] = new InstalledServiceJson
            {
                Id = Guid.NewGuid(),
                Name = Pick(r, ServiceNames),
                Version = $"{r.Next(1, 6)}.{r.Next(0, 21)}.{r.Next(0, 11)}",
                Port = r.Next(1024, 65536),
                Status = Pick(r, ServiceStatuses),
                InstalledAt = DateTime.UtcNow.AddDays(-r.Next(1, 365 * 2)),
            };
        }
        Interlocked.Add(ref _serviceTicks, Stopwatch.GetTimestamp() - ts);
        return arr;
    }

    static ServerTagJson[] GenerateTags(Random r)
    {
        var ts = Stopwatch.GetTimestamp();
        var count = r.Next(1, 6);
        var arr = new ServerTagJson[count];
        for (var i = 0; i < count; i++)
        {
            arr[i] = new ServerTagJson
            {
                Id = Guid.NewGuid(),
                Key = Pick(r, TagKeys),
                Value = Pick(r, TagValues),
            };
        }
        Interlocked.Add(ref _tagTicks, Stopwatch.GetTimestamp() - ts);
        return arr;
    }

    static string Ip(Random r) =>
        $"{r.Next(1, 255)}.{r.Next(0, 256)}.{r.Next(0, 256)}.{r.Next(1, 255)}";

    // string.Create writes directly into the string's char buffer — no intermediate
    // format strings, and one r.NextBytes call instead of six r.Next calls.
    static string Mac(Random r)
    {
        var ts = Stopwatch.GetTimestamp();
        Span<byte> b = stackalloc byte[6];
        r.NextBytes(b);
        var result = string.Create(
            17,
            (b[0], b[1], b[2], b[3], b[4], b[5]),
            static (span, t) =>
            {
                WriteOctet(span, 0, t.Item1);
                span[2] = ':';
                WriteOctet(span, 3, t.Item2);
                span[5] = ':';
                WriteOctet(span, 6, t.Item3);
                span[8] = ':';
                WriteOctet(span, 9, t.Item4);
                span[11] = ':';
                WriteOctet(span, 12, t.Item5);
                span[14] = ':';
                WriteOctet(span, 15, t.Item6);
            }
        );
        Interlocked.Add(ref _macTicks, Stopwatch.GetTimestamp() - ts);
        return result;

        static void WriteOctet(Span<char> s, int pos, byte v)
        {
            s[pos] = HexChar(v >> 4);
            s[pos + 1] = HexChar(v & 0xF);
        }

        static char HexChar(int n) => (char)(n < 10 ? '0' + n : 'a' + n - 10);
    }

    static T Pick<T>(Random r, ReadOnlySpan<T> arr) => arr[r.Next(arr.Length)];
}
