using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using API.DTOs;
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
public class ServersController(TheApiDbContext context, ILogger<ServersController> logger)
    : ControllerBase
{
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
        var servers = FastServerSeeder.Generate(count);
        var generationMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Generation: {GenerationMs}ms", generationMs);

        sw.Restart();
        await BulkInsertAsync(servers, ct);
        var insertMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Bulk insert: {InsertMs}ms", insertMs);

        return Accepted(
            new
            {
                saved = count,
                generationMs,
                insertMs,
            }
        );
    }

    async Task BulkInsertAsync(ServerJsonEntity[] servers, CancellationToken ct)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(Guid));
        dt.Columns.Add("Hostname", typeof(string));
        dt.Columns.Add("IpAddress", typeof(string));
        dt.Columns.Add("OperatingSystem", typeof(string));
        dt.Columns.Add("CpuCores", typeof(int));
        dt.Columns.Add("MemoryMb", typeof(int));
        dt.Columns.Add("Status", typeof(string));
        dt.Columns.Add("Environment", typeof(string));
        dt.Columns.Add("ProvisionedAt", typeof(DateTime));
        dt.Columns.Add("DecommissionedAt", typeof(DateTime));
        dt.Columns.Add("Disks", typeof(string));
        dt.Columns.Add("NetworkInterfaces", typeof(string));
        dt.Columns.Add("InstalledServices", typeof(string));
        dt.Columns.Add("Tags", typeof(string));

        foreach (var s in servers)
        {
            dt.Rows.Add(
                s.Id,
                s.Hostname,
                s.IpAddress,
                s.OperatingSystem,
                s.CpuCores,
                s.MemoryMb,
                s.Status,
                s.Environment,
                s.ProvisionedAt,
                (object?)s.DecommissionedAt ?? DBNull.Value,
                JsonSerializer.Serialize(s.Disks),
                JsonSerializer.Serialize(s.NetworkInterfaces),
                JsonSerializer.Serialize(s.InstalledServices),
                JsonSerializer.Serialize(s.Tags)
            );
        }

        await using var conn = new SqlConnection(context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using (var tx = conn.BeginTransaction())
        {
            await using (var cmd = new SqlCommand("TRUNCATE TABLE ServersJson", conn, tx))
                await cmd.ExecuteNonQueryAsync(ct);

            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, tx))
            {
                bulk.DestinationTableName = "ServersJson";
                bulk.BatchSize = 0;
                bulk.BulkCopyTimeout = 120;

                foreach (DataColumn col in dt.Columns)
                {
                    bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }

                await bulk.WriteToServerAsync(dt, ct);
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
        var sw = Stopwatch.StartNew();
        var servers = FastServerSeeder.Generate(count);
        var generationMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Generation: {GenerationMs}ms", generationMs);

        sw.Restart();
        await context.TruncateAsync<ServerJsonEntity>(cancellationToken: ct);
        await context.BulkInsertOrUpdateOrDeleteAsync(servers, cancellationToken: ct);
        var insertMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("BulkExt insert: {InsertMs}ms", insertMs);

        return Accepted(
            new
            {
                saved = count,
                generationMs,
                insertMs,
            }
        );
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
