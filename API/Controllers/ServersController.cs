using System.Diagnostics;
using API.DTOs;
using Common;
using Common.Entities;
using Common.Seeding;
using Microsoft.AspNetCore.Mvc;
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

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        sw.Restart();
        var saved = 0;
        for (var i = 0; i < servers.Length; i += 500)
        {
            context.ServersJson.AddRange(servers.Skip(i).Take(500));
            saved += await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
        var efMs = sw.Elapsed.TotalMilliseconds;
        logger.LogInformation("EF insert: {EfMs}ms", efMs);

        context.ChangeTracker.AutoDetectChangesEnabled = true;

        return Accepted(
            new
            {
                saved,
                generationMs,
                efMs,
            }
        );
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
