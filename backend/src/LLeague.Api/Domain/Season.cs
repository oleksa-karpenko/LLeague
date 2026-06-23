namespace LLeague.Api.Domain;

public class Season
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Year { get; set; }

    public List<Event> Events { get; set; } = [];
}

