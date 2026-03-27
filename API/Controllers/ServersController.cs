
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
            .ToArrayAsync();
    }
    
    [HttpGet("tag")]
    public async Task<object> GetVolume()
    {
        return await context.ServerTags.FirstAsync();
    }
}