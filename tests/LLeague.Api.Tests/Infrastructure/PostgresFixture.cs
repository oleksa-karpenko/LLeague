using LLeague.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace LLeague.Api.Tests.Infrastructure;

/// <summary>
/// One Postgres container + API host shared by the whole integration collection.
/// Started once, torn down once. Tests isolate themselves by creating their own
/// season/event/division graph (see <see cref="TestApi"/>).
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public ApiFactory Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        Factory = new ApiFactory(_db.GetConnectionString());

        // Migrate + seed exactly once against the fresh container (the app hosts skip this).
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_db.GetConnectionString())
            .Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, TestApi.AdminPassword);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _db.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "api";
}
