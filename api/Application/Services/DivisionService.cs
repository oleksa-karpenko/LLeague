using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class DivisionService(IAppDbContext db)
{
    // ---------- Divisions ----------

    public async Task<IEnumerable<DivisionResponse>> ListAsync(Guid? eventId)
    {
        IQueryable<Division> q = db.Divisions.AsQueryable();
        if (eventId is Guid eid)
        {
            q = q.Where(d => d.EventId == eid);
        }

        return await q.OrderBy(d => d.Name).Select(d => Map(d)).ToListAsync();
    }

    public async Task<DivisionResponse> GetAsync(Guid id)
    {
        Division d = await db.Divisions.FindAsync(id) ?? throw new NotFoundException();
        return Map(d);
    }

    public async Task<DivisionResponse> CreateAsync(DivisionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }

        if (!await db.Events.AnyAsync(e => e.Id == req.EventId))
        {
            throw new ValidationException("Event not found");
        }

        var d = new Division
        {
            Id = Guid.NewGuid(),
            EventId = req.EventId,
            Name = req.Name.Trim(),
            Color = string.IsNullOrWhiteSpace(req.Color) ? "#1f6feb" : req.Color.Trim()
        };
        _ = db.Divisions.Add(d);
        _ = await db.SaveChangesAsync();
        return Map(d);
    }

    public async Task<DivisionResponse> UpdateAsync(Guid id, DivisionRequest req)
    {
        Division d = await db.Divisions.FindAsync(id) ?? throw new NotFoundException();
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }

        d.Name = req.Name.Trim();
        d.Color = string.IsNullOrWhiteSpace(req.Color) ? d.Color : req.Color.Trim();
        _ = await db.SaveChangesAsync();
        return Map(d);
    }

    public async Task DeleteAsync(Guid id)
    {
        Division d = await db.Divisions.FindAsync(id) ?? throw new NotFoundException();
        _ = db.Divisions.Remove(d);
        _ = await db.SaveChangesAsync();
    }

    // ---------- Team enrollment ----------

    public async Task<IEnumerable<EnrolledTeamResponse>> EnrolledTeamsAsync(Guid id)
    {
        await EnsureDivisionExists(id);
        return await db.TeamDivisions
            .Where(td => td.DivisionId == id)
            .OrderBy(td => td.Team!.Number)
            .Select(td => new EnrolledTeamResponse(
                td.TeamId, td.Team!.Number, td.Team.Name, td.Team.Affiliation,
                td.Team.City, td.Team.Region, td.Arrived, td.ArrivedAt))
            .ToListAsync();
    }

    public async Task EnrollAsync(Guid id, EnrollRequest req)
    {
        await EnsureDivisionExists(id);
        if (!await db.Teams.AnyAsync(t => t.Id == req.TeamId))
        {
            throw new ValidationException("Team not found");
        }

        if (await db.TeamDivisions.AnyAsync(td => td.DivisionId == id && td.TeamId == req.TeamId))
        {
            throw new ConflictException("Team is already enrolled in this division");
        }

        _ = db.TeamDivisions.Add(new TeamDivision
        {
            Id = Guid.NewGuid(),
            DivisionId = id,
            TeamId = req.TeamId,
            Arrived = false
        });
        _ = await db.SaveChangesAsync();
    }

    public async Task MarkArrivalAsync(Guid id, Guid teamId)
    {
        TeamDivision td = await db.TeamDivisions.FirstOrDefaultAsync(x => x.DivisionId == id && x.TeamId == teamId)
            ?? throw new NotFoundException();
        td.Arrived = true;
        td.ArrivedAt = DateTimeOffset.UtcNow;
        _ = await db.SaveChangesAsync();
    }

    // ---------- Tables ----------

    public async Task<IEnumerable<TableResponse>> TablesAsync(Guid id)
    {
        await EnsureDivisionExists(id);
        return await db.Tables.Where(t => t.DivisionId == id)
            .OrderBy(t => t.Name)
            .Select(t => new TableResponse(t.Id, t.Name))
            .ToListAsync();
    }

    public async Task<TableResponse> CreateTableAsync(Guid id, TableRequest req)
    {
        await EnsureDivisionExists(id);
        if (string.IsNullOrWhiteSpace(req.Name))
        {
            throw new ValidationException("Name is required");
        }

        var t = new RobotGameTable { Id = Guid.NewGuid(), DivisionId = id, Name = req.Name.Trim() };
        _ = db.Tables.Add(t);
        _ = await db.SaveChangesAsync();
        return new TableResponse(t.Id, t.Name);
    }

    // ---------- Standings ----------

    public async Task<IEnumerable<StandingRow>> StandingsAsync(Guid id)
    {
        await EnsureDivisionExists(id);

        // Pull the scored ranking-round results (translatable to SQL)...
        List<RankingScore> scored = await db.MatchParticipants
            .Where(p => p.Match!.DivisionId == id
                        && p.Match.Stage == MatchStage.Ranking
                        && p.Scoresheet != null
                        && p.Scoresheet.Status == ScoresheetStatus.Completed)
            .Select(p => new RankingScore(p.TeamId, p.Team!.Number, p.Team.Name, p.Scoresheet!.Score))
            .ToListAsync();

        // ...then rank (pure domain) and map to the response DTO.
        return [.. StandingsCalculator.Rank(scored)
            .Select(e => new StandingRow(
                e.Rank, e.TeamId, e.TeamNumber, e.TeamName, e.BestScore, e.TotalScore, e.MatchesPlayed))];
    }

    private async Task EnsureDivisionExists(Guid id)
    {
        if (!await db.Divisions.AnyAsync(d => d.Id == id))
        {
            throw new NotFoundException();
        }
    }

    private static DivisionResponse Map(Division d) => new(d.Id, d.EventId, d.Name, d.Color);
}
