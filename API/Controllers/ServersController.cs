
using Common.Entities;
using Common.Seeding;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ServersController : ControllerBase
{
    // GET
    public IAsyncEnumerable<ServerEntity> Index()
    {
        ServerSeeder.Generate()
    }
}