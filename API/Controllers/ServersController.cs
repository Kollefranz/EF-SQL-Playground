using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using API.DTOs;
using API.Metrics;
using Common;
using Common.Entities;
using Common.Seeding;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ServersController(
    TheApiDbContext context,
    ILogger<ServersController> logger,
    SeedingMetrics metrics
) : ControllerBase
{
    const double LogIntervalSeconds = 5.0;
    static readonly KeyValuePair<string, object?> TagSeed = new("endpoint", "seed");
    static readonly KeyValuePair<string, object?> TagBulkExt = new("endpoint", "seed-bulk-ext");

    [HttpGet("seed")]
    public IAsyncEnumerable<ServerJsonEntity> GetSeed(
        [FromQuery] [Range(1, long.MaxValue)] long count = 500
    )
    {
        return ServerSeeder.Generate(count).ToAsyncEnumerable();
    }

    [HttpPost("seed")]
    public async Task<IActionResult> PostSeed(
        CancellationToken ct,
        [FromQuery] [Range(1, long.MaxValue)] long count = 500
    )
    {
        var sw = Stopwatch.StartNew();
        await BulkInsertAsync(count, ct);
        logger.LogInformation("Bulk insert total: {TotalMs:N0}ms", sw.Elapsed.TotalMilliseconds);

        return Accepted(new { saved = count, totalMs = sw.Elapsed.TotalMilliseconds });
    }

    async Task BulkInsertAsync(long count, CancellationToken ct)
    {
        await using var conn = new SqlConnection(context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using (var tx = conn.BeginTransaction())
        {
            var truncateSw = Stopwatch.StartNew();
            await using (var cmd = new SqlCommand("TRUNCATE TABLE ServersJson", conn, tx))
                await cmd.ExecuteNonQueryAsync(ct);
            var truncateSeconds = truncateSw.Elapsed.TotalSeconds;
            logger.LogInformation("Truncate: {TruncateMs:N0}ms", truncateSeconds * 1000);
            metrics.InsertTruncate.Record(truncateSeconds, TagSeed);

            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, tx))
            {
                bulk.DestinationTableName = "ServersJson";
                bulk.BatchSize = 50_000;
                bulk.BulkCopyTimeout = 300;

                var insertSw = Stopwatch.StartNew();
                var logTimer = Stopwatch.StartNew();
                long lastRows = 0;

                // Called every progressEvery rows by ServerDataReader.
                // Always drains sub-method timings; throttles logging by time.
                void OnProgress(long rows)
                {
                    var delta = rows - lastRows;
                    var subTimings = FastServerSeeder.DrainTimings();
                    metrics.RowsSeeded.Add(delta, TagSeed);
                    metrics.RecordSubTimings(subTimings, TagSeed);
                    metrics.SetProgress(rows, count);
                    lastRows = rows;

                    if (rows < count && logTimer.Elapsed.TotalSeconds < LogIntervalSeconds)
                        return;
                    var elapsed = insertSw.Elapsed.TotalSeconds;
                    var rate = rows / elapsed;
                    logger.LogInformation(
                        "Seeding /seed: {Rows:N0}/{Count:N0} | {Elapsed:N1}s | {Rate:N0} rows/s",
                        rows,
                        count,
                        elapsed,
                        rate
                    );
                    metrics.RowsPerSecond.Record(rate, TagSeed);
                    logTimer.Restart();
                }

                var reader = new ServerDataReader(count, OnProgress);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    bulk.ColumnMappings.Add(reader.GetName(i), reader.GetName(i));
                }

                await bulk.WriteToServerAsync(reader, ct);
                OnProgress(count);
                metrics.OperationDuration.Record(insertSw.Elapsed.TotalSeconds, TagSeed);
                metrics.SetProgress(0, 1);
            }

            await tx.CommitAsync(ct);
        }
    }

    [HttpPost("seed-bulk-ext")]
    public async Task<IActionResult> PostSeedBulkExt(
        CancellationToken ct,
        [FromQuery] [Range(1, long.MaxValue)] long count = 500
    )
    {
        var truncateSw = Stopwatch.StartNew();
        await context.TruncateAsync<ServerJsonEntity>(cancellationToken: ct);
        var truncateSeconds = truncateSw.Elapsed.TotalSeconds;
        logger.LogInformation("Truncate: {TruncateMs:N0}ms", truncateSeconds * 1000);
        metrics.InsertTruncate.Record(truncateSeconds, TagBulkExt);

        const int batchSize = 50_000;
        long inserted = 0;
        var totalSw = Stopwatch.StartNew();
        var logTimer = Stopwatch.StartNew();

        // Fixed buffer reused every batch — only the last (possibly smaller) batch
        // creates a new array via buffer[..batchLen].
        var buffer = new ServerJsonEntity[batchSize];
        using var enumerator = FastServerSeeder.GenerateLazy(count, ct).GetEnumerator();

        while (true)
        {
            // --- Generation phase (timed separately from insert) ---
            var genSw = Stopwatch.StartNew();
            var batchLen = 0;
            while (batchLen < batchSize && enumerator.MoveNext())
            {
                buffer[batchLen++] = enumerator.Current;
            }
            if (batchLen == 0)
            {
                break;
            }

            metrics.GenerateBatch.Record(genSw.Elapsed.TotalSeconds, TagBulkExt);
            metrics.RecordSubTimings(FastServerSeeder.DrainTimings(), TagBulkExt);

            // --- Insert phase ---
            var batchToInsert = batchLen == batchSize ? buffer : buffer[..batchLen];
            var insertSw = Stopwatch.StartNew();
            await context.BulkInsertAsync(batchToInsert, cancellationToken: ct);
            metrics.InsertBatch.Record(insertSw.Elapsed.TotalSeconds, TagBulkExt);

            inserted += batchLen;
            metrics.RowsSeeded.Add(batchLen, TagBulkExt);
            metrics.SetProgress(inserted, count);

            if (inserted >= count || logTimer.Elapsed.TotalSeconds >= LogIntervalSeconds)
            {
                var rate = inserted / totalSw.Elapsed.TotalSeconds;
                logger.LogInformation(
                    "Seeding /seed-bulk-ext: {Inserted:N0}/{Count:N0} | {Elapsed:N1}s | {Rate:N0} rows/s",
                    inserted,
                    count,
                    totalSw.Elapsed.TotalSeconds,
                    rate
                );
                metrics.RowsPerSecond.Record(rate, TagBulkExt);
                logTimer.Restart();
            }
        }

        metrics.OperationDuration.Record(totalSw.Elapsed.TotalSeconds, TagBulkExt);
        metrics.SetProgress(0, 1);

        return Accepted(new { saved = count, insertMs = totalSw.Elapsed.TotalMilliseconds });
    }

    [HttpPost("seed-ef")]
    public async Task<IActionResult> PostSeedEf(
        CancellationToken ct,
        [FromQuery] [Range(1, long.MaxValue)] long count = 500
    )
    {
        var sw = Stopwatch.StartNew();
        var servers = FastServerSeeder.Generate(count);
        var generationMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Generation: {GenerationMs}ms", generationMs);

        sw.Restart();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        var saved = 0;
        for (var i = 0; i < servers.Length; i += 500)
        {
            context.ServersJson.AddRange(servers.Skip(i).Take(500));
            saved += await context.SaveChangesAsync(ct);
            context.ChangeTracker.Clear();
        }

        context.ChangeTracker.AutoDetectChangesEnabled = true;
        var efMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("EF insert: {EfMs}ms", efMs);

        return Accepted(
            new
            {
                saved,
                generationMs,
                efMs,
            }
        );
    }

    [HttpGet]
    public IAsyncEnumerable<ServerJsonEntity> GetServers()
    {
        return context.ServersJson.AsNoTracking().AsAsyncEnumerable();
    }

    [HttpGet("paged")]
    public async Task<PagedResult<object>> GetServersPaged(
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25
    )
    {
        var total = await context.Servers.CountAsync(ct);
        var items = await context
            .ServersJson.AsNoTracking()
            // .AsSplitQuery()
            .Include(x => x.NetworkInterfaces)
            .Include(x => x.Disks)
            .Include(x => x.Tags)
            .Include(x => x.InstalledServices)
            .OrderBy(x => x.RowId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<object>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    [HttpGet("tag-infos")]
    public async Task<IActionResult> GetTagInfos(CancellationToken ct)
    {
        var query = context
            .ServerTags.AsNoTracking()
            .Where(x => x.Server != null)
            .Select(x => new TagInfoDto
            {
                ServerId = x.Server!.Id,
                TagName = x.Key,
                TagValue = x.Value,
            });

        return Ok(await query.ToArrayAsync(ct));
    }
}
