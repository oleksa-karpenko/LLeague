using LLeague.Api.Domain;
using Xunit;

namespace LLeague.Api.Tests.Domain;

/// <summary>Unit tests for the pure standings ranking extracted from the controller.</summary>
public class StandingsCalculatorTests
{
    [Fact]
    public void Ranks_by_best_then_total_with_played_count()
    {
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        RankingScore[] scores =
        [
            new(teamA, 2, "A", 50),
            new(teamA, 2, "A", 20),
            new(teamB, 1, "B", 40),
            new(teamB, 1, "B", 30),
        ];

        List<StandingEntry> rows = StandingsCalculator.Rank(scores);

        Assert.Equal(2, rows.Count);

        Assert.Equal(1, rows[0].Rank);
        Assert.Equal(teamA, rows[0].TeamId);   // best 50 beats best 40
        Assert.Equal(50, rows[0].BestScore);
        Assert.Equal(70, rows[0].TotalScore);
        Assert.Equal(2, rows[0].MatchesPlayed);

        Assert.Equal(2, rows[1].Rank);
        Assert.Equal(teamB, rows[1].TeamId);
        Assert.Equal(40, rows[1].BestScore);
        Assert.Equal(70, rows[1].TotalScore);
    }

    [Fact]
    public void Ties_on_best_and_total_break_by_lowest_team_number()
    {
        var lower = Guid.NewGuid();
        var higher = Guid.NewGuid();
        RankingScore[] scores =
        [
            new(higher, 5, "Five", 10),
            new(lower, 3, "Three", 10),
        ];

        List<StandingEntry> rows = StandingsCalculator.Rank(scores);

        Assert.Equal(lower, rows[0].TeamId);    // team number 3 ranks ahead of 5
        Assert.Equal(higher, rows[1].TeamId);
    }

    [Fact]
    public void Empty_input_produces_no_rows()
    {
        Assert.Empty(StandingsCalculator.Rank([]));
    }
}
