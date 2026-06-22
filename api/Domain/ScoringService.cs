using System.Globalization;

namespace LLeague.Api.Domain;

/// <summary>Computes a scoresheet's score from its clause values, using the MissionCatalog.</summary>
public class ScoringService
{
    public int ComputeScore(IEnumerable<MissionValue> values)
    {
        var total = 0;

        foreach (MissionValue v in values)
        {
            if (!MissionCatalog.ById.TryGetValue(v.MissionId, out MissionDef? mission))
            {
                continue;                                    // unknown mission -> ignore
            }

            MissionClauseDef? clause = mission.Clauses.FirstOrDefault(c => c.Index == v.ClauseIndex);
            if (clause is null)
            {
                continue;
            }

            total += clause.Type switch
            {
                "boolean" => ParseBool(v.ValueRaw) ? (clause.Points ?? 0) : 0,
                "number" => ScoreNumber(clause, v.ValueRaw),
                "enum" => ScoreEnum(clause, v.ValueRaw),
                _ => 0
            };
        }

        return Math.Max(0, total);   // never negative
    }

    private static bool ParseBool(string? raw)
    {
        return bool.TryParse(raw, out var b) && b;
    }

    private static int ScoreNumber(MissionClauseDef clause, string? raw)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < 0)
        {
            return 0;
        }

        if (clause.Max is int max)
        {
            n = Math.Min(n, max);     // apply the cap
        }

        return n * (clause.PerUnit ?? 0);
    }

    private static int ScoreEnum(MissionClauseDef clause, string? raw)
    {
        ClauseOption? opt = clause.Options?.FirstOrDefault(o => o.Value == raw);
        return opt?.Points ?? 0;
    }
}
