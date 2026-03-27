
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
    public IAsyncEnumerable<ServerEntity> GetSeed([FromQuery] ushort count = 500)
    {
        return ServerSeeder.Generate(count).ToAsyncEnumerable();
    }
    
    
    [HttpPost("seed")]
    public async Task<IActionResult> PostSeed([FromQuery] ushort count = 500)
    {
        var servers = ServerSeeder.Generate(count);
        
        await context.Servers.AddRangeAsync(servers);
        var res = await context.SaveChangesAsync();

        return Accepted(res);
    }


    [HttpGet()]
    public async Task<object> GetServers()
    {
        return await context.Servers
            .Include(x => x.NetworkInterfaces)
            .Include(x => x.Disks)
            .Include(x => x.Tags)
            .Include(x => x.InstalledServices)
            .ToArrayAsync();
    }
    
    [HttpGet("tag-infos")]
    public IAsyncEnumerable<TagInfoDto> GetTagInfos()
    {
        return context.ServerTags
            .AsNoTracking()
            .Select(x => new TagInfoDto
            {
                ServerId = x.ServerId,
                TagName = x.Key,
                TagValue = x.Value,
            })
            .AsAsyncEnumerable();
    }
}