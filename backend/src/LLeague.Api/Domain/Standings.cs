namespace LLeague.Api.Domain;

public readonly record struct RankingScore(Guid TeamId, int TeamNumber, string TeamName, int Score);

public readonly record struct StandingEntry(
    int Rank, Guid TeamId, int TeamNumber, string TeamName, int BestScore, int TotalScore, int MatchesPlayed);

public static class StandingsCalculator
{
    /// <summary>
    /// Aggregate per-team scores and rank them: highest single (Best) score first,
    /// then highest Total, then lowest team number.
    /// </summary>
    public static List<StandingEntry> Rank(IEnumerable<RankingScore> scores)
    {
        return [.. scores
            .GroupBy(x => new { x.TeamId, x.TeamNumber, x.TeamName })
            .Select(g => new
            {
                g.Key.TeamId,
                g.Key.TeamNumber,
                g.Key.TeamName,
                Best = g.Max(x => x.Score),
                Total = g.Sum(x => x.Score),
                Played = g.Count()
            })
            .OrderByDescending(r => r.Best)
            .ThenByDescending(r => r.Total)
            .ThenBy(r => r.TeamNumber)
            .Select((r, i) => new StandingEntry(
                i + 1, r.TeamId, r.TeamNumber, r.TeamName, r.Best, r.Total, r.Played))];
    }
}
