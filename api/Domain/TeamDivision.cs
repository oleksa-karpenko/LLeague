namespace LLeague.Api.Domain;

public class TeamDivision
{
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid DivisionId { get; set; }
    public Division? Division { get; set; }

    public bool Arrived { get; set; }
    public DateTimeOffset? ArrivedAt { get; set; }
}
