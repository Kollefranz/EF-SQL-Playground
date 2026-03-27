using Common;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddTheApiDatabase(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapOpenApi();
app.MapScalarApiReference(string.Empty);

app.UseAuthorization();

app.MapControllers();

app.Run();
