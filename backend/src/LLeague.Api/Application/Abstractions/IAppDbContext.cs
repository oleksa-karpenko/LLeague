using LLeague.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Abstractions;

/// <summary>
/// Persistence seam for the application layer. Exposes the aggregate sets and the
/// unit-of-work save, without binding callers to the concrete EF <c>AppDbContext</c>
/// (which lives in Infrastructure). EF Core remains the unit of work — no repositories.
/// </summary>
public interface IAppDbContext
{
    DbSet<AdminUser> AdminUsers { get; }
    DbSet<Division> Divisions { get; }
    DbSet<Event> Events { get; }
    DbSet<Match> Matches { get; }
    DbSet<MatchParticipant> MatchParticipants { get; }
    DbSet<MissionValue> MissionValues { get; }
    DbSet<RobotGameTable> Tables { get; }
    DbSet<Scoresheet> Scoresheets { get; }
    DbSet<Season> Seasons { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamDivision> TeamDivisions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
