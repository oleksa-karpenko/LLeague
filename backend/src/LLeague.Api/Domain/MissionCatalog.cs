namespace LLeague.Api.Domain;

public record ClauseOption(string Label, string Value, int Points);

public record MissionClauseDef(
    int Index,
    string Type,                 // "boolean" | "number" | "enum"
    string Label,
    int? Points = null,          // boolean: points awarded when true
    int? PerUnit = null,         // number: points per counted unit
    int? Max = null,             // number: cap on counted units
    List<ClauseOption>? Options = null); // enum: option -> points

public record MissionDef(string MissionId, string Title, List<MissionClauseDef> Clauses);

public static class MissionCatalog
{
    public static readonly List<MissionDef> Missions =
    [
        new("M01", "Surface Brushing",
        [
            new(0, "boolean", "Surface brush released", Points: 10)
        ]),
        new("M02", "Samples Collected",
        [
            new(0, "number", "Number of samples in base", PerUnit: 5, Max: 6)
        ]),
        new("M03", "Artifact Identification",
        [
            new(0, "enum", "Artifact location", Options:
            [
                new("Not identified", "none", 0),
                new("Partial", "partial", 10),
                new("Complete", "complete", 20)
            ])
        ]),
        new("M04", "Equipment Delivery",
        [
            new(0, "boolean", "Equipment in target zone", Points: 15),
            new(1, "number", "Bonus crates stacked", PerUnit: 5, Max: 3)
        ]),
        new("M05", "Precision Tokens",
        [
            new(0, "number", "Precision tokens remaining", PerUnit: 10, Max: 6)
        ])
    ];

    public static readonly Dictionary<string, MissionDef> ById =
        Missions.ToDictionary(m => m.MissionId);
}
