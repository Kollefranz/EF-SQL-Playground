using System.Diagnostics.Metrics;
using Common.Seeding;

namespace API.Metrics;

public sealed class SeedingMetrics : IDisposable
{
    readonly Meter _meter;
    double _progress;

    // ── Throughput ────────────────────────────────────────────────────────────

    /// <summary>Rows successfully written to the database.</summary>
    public readonly Counter<long> RowsSeeded;

    /// <summary>Insert throughput, sampled at each progress drain point.</summary>
    public readonly Histogram<double> RowsPerSecond;

    // ── Operation-level ───────────────────────────────────────────────────────

    /// <summary>Duration of a complete seed operation.</summary>
    public readonly Histogram<double> OperationDuration;

    /// <summary>Live seeding progress, 0–100.</summary>
    public readonly ObservableGauge<double> SeedingProgress;

    // ── Insert phase ──────────────────────────────────────────────────────────

    /// <summary>Time to TRUNCATE the table before seeding.</summary>
    public readonly Histogram<double> InsertTruncate;

    /// <summary>Time to insert one batch into the database.</summary>
    public readonly Histogram<double> InsertBatch;

    // ── Generate phase ────────────────────────────────────────────────────────

    /// <summary>Total time to generate one batch of entities.</summary>
    public readonly Histogram<double> GenerateBatch;

    /// <summary>Time spent inside GenerateDisks across one batch.</summary>
    public readonly Histogram<double> GenerateDisks;

    /// <summary>Time spent inside GenerateNics across one batch (includes Mac).</summary>
    public readonly Histogram<double> GenerateNics;

    /// <summary>Time spent inside GenerateServices across one batch.</summary>
    public readonly Histogram<double> GenerateServices;

    /// <summary>Time spent inside GenerateTags across one batch.</summary>
    public readonly Histogram<double> GenerateTags;

    /// <summary>Time spent inside Mac across one batch.</summary>
    public readonly Histogram<double> GenerateMac;

    public SeedingMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create("api.seeding");

        RowsSeeded = _meter.CreateCounter<long>(
            "seeding.rows_seeded",
            unit: "rows",
            description: "Rows written to the database"
        );
        RowsPerSecond = _meter.CreateHistogram<double>(
            "seeding.rows_per_second",
            unit: "rows/s",
            description: "Insert throughput sampled at each progress interval"
        );
        OperationDuration = _meter.CreateHistogram<double>(
            "seeding.operation_duration",
            unit: "s",
            description: "Duration of a complete seed operation"
        );
        SeedingProgress = _meter.CreateObservableGauge<double>(
            "seeding.progress",
            () => Volatile.Read(ref _progress),
            unit: "%",
            description: "Current seeding progress, 0–100"
        );
        InsertTruncate = _meter.CreateHistogram<double>(
            "seeding.insert.truncate",
            unit: "s",
            description: "Time to truncate the table"
        );
        InsertBatch = _meter.CreateHistogram<double>(
            "seeding.insert.batch",
            unit: "s",
            description: "Time to insert one batch into the database"
        );
        GenerateBatch = _meter.CreateHistogram<double>(
            "seeding.generate.batch",
            unit: "s",
            description: "Time to generate one batch of entities"
        );
        GenerateDisks = _meter.CreateHistogram<double>(
            "seeding.generate.disks",
            unit: "s",
            description: "Time in GenerateDisks across one batch"
        );
        GenerateNics = _meter.CreateHistogram<double>(
            "seeding.generate.nics",
            unit: "s",
            description: "Time in GenerateNics across one batch (includes Mac)"
        );
        GenerateServices = _meter.CreateHistogram<double>(
            "seeding.generate.services",
            unit: "s",
            description: "Time in GenerateServices across one batch"
        );
        GenerateTags = _meter.CreateHistogram<double>(
            "seeding.generate.tags",
            unit: "s",
            description: "Time in GenerateTags across one batch"
        );
        GenerateMac = _meter.CreateHistogram<double>(
            "seeding.generate.mac",
            unit: "s",
            description: "Time in Mac across one batch"
        );
    }

    public void SetProgress(long rows, long total) =>
        Volatile.Write(ref _progress, total == 0 ? 0 : rows * 100.0 / total);

    public void RecordSubTimings(
        FastServerSeeder.SubMethodTimings t,
        KeyValuePair<string, object?> tag
    )
    {
        GenerateDisks.Record(t.DisksSeconds, tag);
        GenerateNics.Record(t.NicsSeconds, tag);
        GenerateServices.Record(t.ServicesSeconds, tag);
        GenerateTags.Record(t.TagsSeconds, tag);
        GenerateMac.Record(t.MacSeconds, tag);
    }

    public void Dispose() => _meter.Dispose();
}
