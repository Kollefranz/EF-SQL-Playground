using Common;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
}).AddEntityFramework();

builder.Services.AddTheApiDatabase(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseMiniProfiler();

app.MapOpenApi();
app.MapScalarApiReference(string.Empty);

app.UseAuthorization();

app.MapControllers();

app.Run();
