using System.CommandLine;
using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CLI.Commands;

public class EchoCommand : Command
{
    public EchoCommand(IServiceProvider services) : base("echo", "Echoes a message and prints the server count from the database")
    {
        var messageArg = new Argument<string>("message") { Description = "The message to echo" };
        Add(messageArg);

        this.SetAction(async (parseResult, ct) =>
        {
            var message = parseResult.GetValue(messageArg);
            Console.WriteLine(message);

            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TheApiDbContext>();
            var count = await db.Servers.CountAsync(ct);
            Console.WriteLine($"(there are {count} servers in the database)");
        });
    }
}
