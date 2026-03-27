using System.CommandLine;
using CLI.Commands;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddUserSecrets<Program>();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddTheApiDatabase(ctx.Configuration.GetConnectionString("DefaultConnection"));
    })
    .Build();

var rootCommand = new RootCommand("TheAPI CLI");
rootCommand.Add(new EchoCommand(host.Services));

return await rootCommand.Parse(args).InvokeAsync();
