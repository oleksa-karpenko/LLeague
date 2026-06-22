namespace LLeague.Api.Domain;

public class MatchParticipant
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }
    public Match? Match { get; set; }

    public Guid TableId { get; set; }
    public RobotGameTable? Table { get; set; }

    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    public bool Ready { get; set; }

    public Scoresheet? Scoresheet { get; set; }   // one-to-one
}
