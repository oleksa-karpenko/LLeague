namespace LLeague.Api.Domain;

public class Team
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = "";
    public string Affiliation { get; set; } = "";
    public string City { get; set; } = "";

    /// <summary>ISO-3166 alpha-2 country code (exactly 2 letters).</summary>
    public string Region { get; set; } = "UA";

    public List<TeamDivision> TeamDivisions { get; set; } = [];
}
