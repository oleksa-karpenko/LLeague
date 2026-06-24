using LLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Infrastructure;

public static class DbSeeder
{
    /// <summary>Seeds the admin user and a small demo dataset. Safe to run repeatedly.</summary>
    public static async Task SeedAsync(AppDbContext db, string adminPassword)
    {
        // --- Admin user (only if none exists) ---
        if (!await db.AdminUsers.AnyAsync())
        {
            _ = db.AdminUsers.Add(new AdminUser
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 12)
            });
            _ = await db.SaveChangesAsync();
        }

        // --- Demo data (only if there are no seasons yet) ---
        if (await db.Seasons.AnyAsync())
        {
            return;
        }

        var season = new Season { Id = Guid.NewGuid(), Name = "Demo Season", Year = 2026 };

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            SeasonId = season.Id,
            Name = "Demo Regional",
            Slug = "demo-regional-2026",
            StartDate = new DateOnly(2026, 3, 14),
            EndDate = new DateOnly(2026, 3, 14),
            Location = "Tel Aviv"
        };

        var division = new Division
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            Name = "Division A",
            Color = "#1f6feb"
        };

        RobotGameTable[] tables =
        [
            new RobotGameTable { Id = Guid.NewGuid(), DivisionId = division.Id, Name = "Table 1" },
            new RobotGameTable { Id = Guid.NewGuid(), DivisionId = division.Id, Name = "Table 2" }
        ];

        Team[] teams =
        [
            new Team { Id = Guid.NewGuid(), Number = 1001, Name = "Robo Lions",  Affiliation = "Lincoln School", City = "Tel Aviv",   Region = "IL" },
            new Team { Id = Guid.NewGuid(), Number = 1002, Name = "Gear Hawks",   Affiliation = "Edison School",  City = "Haifa",      Region = "IL" },
            new Team { Id = Guid.NewGuid(), Number = 1003, Name = "Byte Bots",    Affiliation = "Newton School",  City = "Eilat",      Region = "IL" },
            new Team { Id = Guid.NewGuid(), Number = 1004, Name = "Circuit Crew", Affiliation = "Tesla School",   City = "Beer Sheva", Region = "IL" }
        ];

        _ = db.Seasons.Add(season);
        _ = db.Events.Add(ev);
        _ = db.Divisions.Add(division);
        db.Tables.AddRange(tables);
        db.Teams.AddRange(teams);
        db.TeamDivisions.AddRange(teams.Select(t => new TeamDivision
        {
            Id = Guid.NewGuid(),
            TeamId = t.Id,
            DivisionId = division.Id,
            Arrived = false
        }));

        _ = await db.SaveChangesAsync();
    }
}
