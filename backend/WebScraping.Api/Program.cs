using WebScraping.Api.Endpoints;
using WebScraping.Infrastructure.DependencyInjection;
using WebScraping.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddWebScrapingInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

var useInMemory = app.Configuration.GetValue("Testing:UseInMemoryStores", false);
if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var indexes = scope.ServiceProvider.GetService<MongoIndexInitializer>();
    if (indexes is not null)
    {
        await indexes.EnsureIndexesAsync();
    }
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapSearchesEndpoints();

app.Run();

public partial class Program;
