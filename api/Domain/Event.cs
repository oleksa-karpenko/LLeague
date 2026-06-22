namespace LLeague.Api.Domain;

public class Event
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }      // FK
    public Season? Season { get; set; }     // Navigation
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Location { get; set; } = "";
    public List<Division> Divisions { get; set; } = [];
}
