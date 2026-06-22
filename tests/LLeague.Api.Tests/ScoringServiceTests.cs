using LLeague.Api.Domain;
using Xunit;

namespace LLeague.Api.Tests;

/// <summary>
/// Pure unit tests over <see cref="ScoringService"/> + the current MissionCatalog.
/// These lock the scoring rules (boolean / number-with-cap / enum) before any refactor.
/// </summary>
public class ScoringServiceTests
{
    private readonly ScoringService _scoring = new();

    private static MissionValue Value(string missionId, int clause, string type, string? raw) =>
        new() { MissionId = missionId, ClauseIndex = clause, ValueType = type, ValueRaw = raw };

    [Fact]
    public void Empty_sheet_scores_zero()
    {
        Assert.Equal(0, _scoring.ComputeScore([]));
    }

    [Theory]
    [InlineData("true", 10)]
    [InlineData("false", 0)]
    [InlineData(null, 0)]
    [InlineData("garbage", 0)]
    public void Boolean_clause_awards_points_only_when_true(string? raw, int expected)
    {
        // M01 clause 0 is a boolean worth 10.
        Assert.Equal(expected, _scoring.ComputeScore([Value("M01", 0, "boolean", raw)]));
    }

    [Theory]
    [InlineData("4", 20)]   // 4 * 5
    [InlineData("6", 30)]   // 6 * 5
    [InlineData("99", 30)]  // capped at Max = 6 -> 30
    [InlineData("-3", 0)]   // negative ignored
    [InlineData("x", 0)]    // unparseable ignored
    public void Number_clause_applies_per_unit_and_cap(string raw, int expected)
    {
        // M02 clause 0 is a number: PerUnit = 5, Max = 6.
        Assert.Equal(expected, _scoring.ComputeScore([Value("M02", 0, "number", raw)]));
    }

    [Theory]
    [InlineData("complete", 20)]
    [InlineData("partial", 10)]
    [InlineData("none", 0)]
    [InlineData("bogus", 0)]
    public void Enum_clause_awards_the_matching_option_points(string raw, int expected)
    {
        // M03 clause 0 is an enum: none=0, partial=10, complete=20.
        Assert.Equal(expected, _scoring.ComputeScore([Value("M03", 0, "enum", raw)]));
    }

    [Fact]
    public void Unknown_mission_is_ignored()
    {
        Assert.Equal(0, _scoring.ComputeScore([Value("ZZZ", 0, "boolean", "true")]));
    }

    [Fact]
    public void Scores_sum_across_missions()
    {
        int total = _scoring.ComputeScore(
        [
            Value("M01", 0, "boolean", "true"),  // 10
            Value("M02", 0, "number", "6"),      // 30
            Value("M03", 0, "enum", "complete"), // 20
        ]);
        Assert.Equal(60, total);
    }
}
