using LLeague.Api.Domain.Exceptions;

namespace LLeague.Api.Domain;

public class Match
{
    public Guid Id { get; set; }
    public Guid DivisionId { get; set; }
    public Division? Division { get; set; }

    public int Round { get; set; }
    public int Number { get; set; }
    public MatchStage Stage { get; set; } = MatchStage.Ranking;
    public MatchStatus Status { get; set; } = MatchStatus.NotStarted;

    public DateTimeOffset? ScheduledTime { get; set; }
    public DateTimeOffset? StartTime { get; set; }

    public List<MatchParticipant> Participants { get; set; } = [];

    public void Start()
    {
        EnsureStatus(MatchStatus.NotStarted, MatchStatus.InProgress);
        Status = MatchStatus.InProgress;
        StartTime = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        EnsureStatus(MatchStatus.InProgress, MatchStatus.Completed);
        Status = MatchStatus.Completed;
    }

    public void Abort()
    {
        Status = MatchStatus.NotStarted;
        StartTime = null;
    }

    private void EnsureStatus(MatchStatus from, MatchStatus to)
    {
        if (Status != from)
        {
            throw new ConflictException($"Cannot transition a match from {Status} to {to}");
        }
    }
}
