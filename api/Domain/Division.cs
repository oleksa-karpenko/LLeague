namespace LLeague.Api.Domain;

public class Division
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public string Name { get; set; } = "";
    public string Color { get; set; } = "#if6feb";

    public List<TeamDivision> TeamDivisions { get; set; } = [];
    public List<RobotGameTable> Tables { get; set; } = [];
    public List<Match> Matches { get; set; } = [];
}
