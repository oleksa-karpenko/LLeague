using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LLeague.Api.Tests.Infrastructure;

/// <summary>
/// Boots the real API host pointed at a throwaway Postgres container and supplies the
/// secrets Program.cs requires. Overrides go through UseSetting (host configuration) so
/// they're visible when Program reads them at builder-time — ConfigureAppConfiguration
/// applies too late, which would silently leave the app on the appsettings.json (dev) DB.
/// The fixture migrates+seeds the container once; these hosts skip startup migration.
/// </summary>
public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Postgres", connectionString);
        builder.UseSetting("ADMIN_PASSWORD", TestApi.AdminPassword);
        builder.UseSetting("JWT_SECRET", "test-only-insecure-secret-change-me-please-32+chars");
        builder.UseSetting("RunDbMigration", "false");
    }
}
