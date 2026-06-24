namespace LLeague.Api.Domain;

public class Scoresheet
{
    public Guid Id { get; set; }

    public Guid MatchParticipantId { get; set; }
    public MatchParticipant? MatchParticipant { get; set; }

    public ScoresheetStatus Status { get; set; } = ScoresheetStatus.Empty;
    public int Score { get; set; }

    public List<MissionValue> Missions { get; set; } = [];

    /// <summary>Replace the mission values wholesale and (re)score the sheet server-side.</summary>
    public void Apply(List<MissionValue> missions, ScoringService scoring)
    {
        Missions = missions;
        Score = scoring.ComputeScore(missions);
        if (Status == ScoresheetStatus.Empty)
        {
            Status = ScoresheetStatus.Draft;
        }
    }

    /// <summary>Finalize the sheet, recomputing the score (never trust a stale value).</summary>
    public void Submit(ScoringService scoring)
    {
        Score = scoring.ComputeScore(Missions);
        Status = ScoresheetStatus.Completed;
    }
}

public class MissionValue
{
    public Guid Id { get; set; }

    public Guid ScoresheetId { get; set; }
    public Scoresheet? Scoresheet { get; set; }

    public string MissionId { get; set; } = "";   // e.g. "M01"
    public int ClauseIndex { get; set; }

    /// <summary>"boolean" | "number" | "enum"</summary>
    public string ValueType { get; set; } = "boolean";

    /// <summary>Raw value stored as string; parsed according to ValueType.</summary>
    public string? ValueRaw { get; set; }
}
