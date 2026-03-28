using System.Data;
using System.Diagnostics;
using System.Text.Json;
using API.DTOs;
using Common;
using Common.Entities;
using Common.Seeding;
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
    public IAsyncEnumerable<ServerJsonEntity> GetSeed([FromQuery] ushort count = 500)
    {
        return ServerSeeder.Generate(count).ToAsyncEnumerable();
    }

    [HttpPost("seed")]
    public async Task<IActionResult> PostSeed([FromQuery] ushort count = 500)
    {
        var sw = Stopwatch.StartNew();
        var servers = FastServerSeeder.Generate(count);
        var generationMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Generation: {GenerationMs}ms", generationMs);

        sw.Restart();
        await BulkInsertAsync(servers);
        var insertMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("Bulk insert: {InsertMs}ms", insertMs);

        return Accepted(new { saved = count, generationMs, insertMs });
    }

    async Task BulkInsertAsync(ServerJsonEntity[] servers)
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
        await conn.OpenAsync();

        using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.TableLock, null))
        // using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
        {
            bulk.DestinationTableName = "ServersJson";
            bulk.BatchSize = 0;
            bulk.BulkCopyTimeout = 120;

            foreach (DataColumn col in dt.Columns)
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

            await bulk.WriteToServerAsync(dt);
        }
    }

    [HttpGet()]
    public IAsyncEnumerable<ServerJsonEntity> GetServers()
    {
        return context.ServersJson.AsNoTracking().AsAsyncEnumerable();
    }

    [HttpGet("paged")]
    public async Task<PagedResult<ServerEntity>> GetServersPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25
    )
    {
        var total = await context.Servers.CountAsync();
        var items = await context
            .Servers.AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.NetworkInterfaces)
            .Include(x => x.Disks)
            .Include(x => x.Tags)
            .Include(x => x.InstalledServices)
            .OrderBy(x => x.RowId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ServerEntity>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    [HttpGet("tag-infos")]
    public IAsyncEnumerable<TagInfoDto> GetTagInfos()
    {
        return context
            .ServerTags.AsNoTracking()
            .Select(x => new TagInfoDto
            {
                ServerId = x.Server!.Id,
                TagName = x.Key,
                TagValue = x.Value,
            })
            .AsAsyncEnumerable();
    }
}
