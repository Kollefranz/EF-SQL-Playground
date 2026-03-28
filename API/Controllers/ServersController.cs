
using API.DTOs;
using Common;
using Common.Entities;
using Common.Seeding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ServersController(TheApiDbContext context) : ControllerBase
{
    [HttpGet("seed")]
    public IAsyncEnumerable<ServerJsonEntity> GetSeed([FromQuery] ushort count = 500)
    {
        return ServerSeeder.Generate(count).ToAsyncEnumerable();
    }


    [HttpPost("seed")]
    public async Task<IActionResult> PostSeed([FromQuery] ushort count = 500)
    {
        var servers = ServerSeeder.Generate(count).ToList();

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var saved = 0;
        for (var i = 0; i < servers.Count; i += 500)
        {
            context.ServersJson.AddRange(servers.Skip(i).Take(500));
            saved += await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        context.ChangeTracker.AutoDetectChangesEnabled = true;

        return Accepted(saved);
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
        return context.ServerTags
            .AsNoTracking()
            .Select(x => new TagInfoDto
            {
                ServerId = x.Server!.Id,
                TagName = x.Key,
                TagValue = x.Value,
            })
            .AsAsyncEnumerable();
    }
}
