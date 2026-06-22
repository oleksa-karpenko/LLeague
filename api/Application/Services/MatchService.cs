using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class MatchService(IAppDbContext db)
{
    public async Task<IEnumerable<MatchResponse>> ListAsync(Guid divisionId)
    {
        await EnsureDivisionExists(divisionId);

        List<Match> matches = await db.Matches
            .Where(m => m.DivisionId == divisionId)
            .Include(m => m.Participants).ThenInclude(p => p.Table)
            .Include(m => m.Participants).ThenInclude(p => p.Team)
            .Include(m => m.Participants).ThenInclude(p => p.Scoresheet)
            .OrderBy(m => m.Round).ThenBy(m => m.Number)
            .ToListAsync();

        return [.. matches.Select(MapMatch)];
    }

    public async Task<MatchResponse> CreateAsync(Guid divisionId, MatchRequest req)
    {
        await EnsureDivisionExists(divisionId);

        if (!Enum.TryParse(req.Stage, ignoreCase: true, out MatchStage stage))
        {
            throw new ValidationException("Stage must be Practice, Ranking or Test");
        }

        if (req.Participants is null || req.Participants.Count == 0)
        {
            throw new ValidationException("A match needs at least one participant");
        }

        // All tables must belong to this division.
        var tableIds = req.Participants.Select(p => p.TableId).ToHashSet();
        if (await db.Tables.CountAsync(t => t.DivisionId == divisionId && tableIds.Contains(t.Id)) != tableIds.Count)
        {
            throw new ValidationException("One or more tables do not belong to this division");
        }

        // All teams must be enrolled in this division.
        var teamIds = req.Participants.Select(p => p.TeamId).ToHashSet();
        if (await db.TeamDivisions.CountAsync(td => td.DivisionId == divisionId && teamIds.Contains(td.TeamId)) != teamIds.Count)
        {
            throw new ValidationException("All teams must be enrolled in this division");
        }

        // A table can host only one team in a match.
        if (req.Participants.Select(p => p.TableId).Distinct().Count() != req.Participants.Count)
        {
            throw new ValidationException("Each table can host only one team in a match");
        }

        var match = new Match
        {
            Id = Guid.NewGuid(),
            DivisionId = divisionId,
            Round = req.Round,
            Number = req.Number,
            Stage = stage,
            Status = MatchStatus.NotStarted,
            Participants = [.. req.Participants.Select(p => new MatchParticipant
            {
                Id = Guid.NewGuid(),
                TableId = p.TableId,
                TeamId = p.TeamId
            })]
        };
        _ = db.Matches.Add(match);
        _ = await db.SaveChangesAsync();

        // Re-query with navigation props loaded for the response.
        Match created = await db.Matches
            .Include(m => m.Participants).ThenInclude(p => p.Table)
            .Include(m => m.Participants).ThenInclude(p => p.Team)
            .FirstAsync(m => m.Id == match.Id);
        return MapMatch(created);
    }

    public Task StartAsync(Guid id) => Transition(id, m => m.Start());
    public Task CompleteAsync(Guid id) => Transition(id, m => m.Complete());
    public Task AbortAsync(Guid id) => Transition(id, m => m.Abort());

    private async Task Transition(Guid id, Action<Match> transition)
    {
        Match m = await db.Matches.FindAsync(id) ?? throw new NotFoundException();
        transition(m);   // invalid transition throws ConflictException
        _ = await db.SaveChangesAsync();
    }

    private async Task EnsureDivisionExists(Guid divisionId)
    {
        if (!await db.Divisions.AnyAsync(d => d.Id == divisionId))
        {
            throw new NotFoundException();
        }
    }

    private static MatchResponse MapMatch(Match m) => new(
        m.Id, m.Round, m.Number, m.Stage.ToString(), m.Status.ToString(),
        m.ScheduledTime, m.StartTime,
        [.. m.Participants.OrderBy(p => p.Table?.Name).Select(p => new MatchParticipantResponse(
            p.Id, p.TableId, p.Table?.Name ?? "", p.TeamId, p.Team?.Number ?? 0, p.Team?.Name ?? "",
            p.Ready,
            (p.Scoresheet?.Status ?? ScoresheetStatus.Empty).ToString(),
            p.Scoresheet?.Score ?? 0))]);
}
