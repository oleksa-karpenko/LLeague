using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class ScoresheetService(IAppDbContext db, ScoringService scoring)
{
    public async Task<ScoresheetResponse> GetAsync(Guid participantId)
    {
        MatchParticipant participant = await LoadParticipant(participantId);
        Scoresheet? sheet = participant.Scoresheet;
        return sheet is null
            ? new ScoresheetResponse(participantId, ScoresheetStatus.Empty.ToString(), 0, [])
            : MapSheet(sheet);
    }

    public async Task<ScoresheetResponse> UpsertAsync(Guid participantId, ScoresheetUpdateRequest req)
    {
        MatchParticipant participant = await LoadParticipant(participantId);

        Scoresheet? sheet = participant.Scoresheet;
        if (sheet is null)
        {
            sheet = new Scoresheet
            {
                Id = Guid.NewGuid(),
                MatchParticipantId = participantId,
                Status = ScoresheetStatus.Draft
            };
            _ = db.Scoresheets.Add(sheet);
        }

        // Replace the mission values wholesale (delete old rows, then apply the new set).
        if (sheet.Missions.Count > 0)
        {
            db.MissionValues.RemoveRange(sheet.Missions);
        }

        List<MissionValue> missions = [.. (req.Missions ?? []).Select(m => new MissionValue
        {
            Id = Guid.NewGuid(),
            ScoresheetId = sheet.Id,
            MissionId = m.MissionId,
            ClauseIndex = m.ClauseIndex,
            ValueType = m.ValueType,
            ValueRaw = m.Value
        })];

        sheet.Apply(missions, scoring);
        _ = await db.SaveChangesAsync();
        return MapSheet(sheet);
    }

    public async Task<ScoresheetResponse> SubmitAsync(Guid participantId)
    {
        MatchParticipant participant = await LoadParticipant(participantId);
        Scoresheet sheet = participant.Scoresheet
            ?? throw new ValidationException("Nothing to submit — score the sheet first");

        sheet.Submit(scoring);
        _ = await db.SaveChangesAsync();
        return MapSheet(sheet);
    }

    private async Task<MatchParticipant> LoadParticipant(Guid participantId)
    {
        return await db.MatchParticipants
            .Include(p => p.Scoresheet).ThenInclude(s => s!.Missions)
            .FirstOrDefaultAsync(p => p.Id == participantId)
            ?? throw new NotFoundException();
    }

    private static ScoresheetResponse MapSheet(Scoresheet s) => new(
        s.MatchParticipantId,
        s.Status.ToString(),
        s.Score,
        [.. s.Missions
            .OrderBy(m => m.MissionId).ThenBy(m => m.ClauseIndex)
            .Select(m => new MissionValueDto(m.MissionId, m.ClauseIndex, m.ValueType, m.ValueRaw))]);
}
