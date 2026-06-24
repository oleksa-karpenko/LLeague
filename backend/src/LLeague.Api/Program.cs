using System.Reflection;
using LLeague.Api.Application;
using LLeague.Api.Infrastructure;
using LLeague.Api.Middleware;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

const string corsPolicy = "spa";
builder.Services.AddCors(o => o.AddPolicy(
    corsPolicy, p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

WebApplication app = builder.Build();

// ---- Apply pending migrations on startup (retry while Postgres is still booting) ----
// Defaults to true; tests disable it ("RunDbMigration": false) and migrate/seed themselves
// once, deterministically — avoids the WebApplicationFactory double-start migration race.
if (builder.Configuration.GetValue("RunDbMigration", true))
{
    var adminPassword = builder.Configuration["ADMIN_PASSWORD"]
        ?? throw new InvalidOperationException("Missing 'ADMIN_PASSWORD'.");
    await MigrateAndSeedAsync(app, adminPassword);
}

// ---- HTTP request pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();   // maps domain exceptions -> status codes
if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}
app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();    // reads & validates the bearer token
app.UseAuthorization();     // enforces [Authorize]
app.MapControllers();

// ---- Build info ----
// Reports the running build's version, set by MinVer from the latest 'v*' git tag.
string apiVersion = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
    ?? "unknown";
app.MapGet("/version", () => Results.Ok(new { version = apiVersion }))
    .AllowAnonymous()
    .WithName("GetVersion");

app.Run();


static async Task MigrateAndSeedAsync(WebApplication app, string adminPassword)
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            await DbSeeder.SeedAsync(db, adminPassword);
            logger.LogInformation("Database migrated and seeded.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning("DB not ready (attempt {Attempt}/{Max}): {Message}. Retrying in 3s...",
                attempt, maxAttempts, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
