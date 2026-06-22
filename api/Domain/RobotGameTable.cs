namespace LLeague.Api.Domain;

public class RobotGameTable
{
    public Guid Id { get; set; }
    public Guid DivisionId { get; set; }
    public Division? Division { get; set; }

    public string Name { get; set; } = "";
}
