using System.Net.Http.Json;
using LLeague.Api.Application;
using LLeague.Api.Tests.Infrastructure;
using Xunit;

namespace LLeague.Api.Tests;

/// <summary>
/// Locks the standings ranking: only completed ranking-stage scoresheets count,
/// rows rank by Best score, then Total, then team number.
/// </summary>
[Collection(ApiCollection.Name)]
public class StandingsTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Standings_rank_by_best_then_total_with_played_count()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync(count: 2);
        Guid teamA = g.TeamIds[0];
        Guid teamB = g.TeamIds[1];

        // Round 1: A = 50 (M03 complete 20 + M02 x6 30), B = 30 (M02 x6)
        await PlayRound(c, g, round: 1,
            aMissions: [Enum("M03", "complete"), Number("M02", "6")],
            bMissions: [Number("M02", "6")]);

        // Round 2: A = 20 (M03 complete), B = 40 (M01 true 10 + M02 x6 30)
        await PlayRound(c, g, round: 2,
            aMissions: [Enum("M03", "complete")],
            bMissions: [Bool("M01", "true"), Number("M02", "6")]);

        List<StandingRow> rows = await (await c.GetAsync($"/divisions/{g.DivisionId}/standings"))
            .ReadAsync<List<StandingRow>>();

        Assert.Equal(2, rows.Count);

        StandingRow first = rows[0];
        Assert.Equal(1, first.Rank);
        Assert.Equal(teamA, first.TeamId);
        Assert.Equal(50, first.BestScore);
        Assert.Equal(70, first.TotalScore);
        Assert.Equal(2, first.MatchesPlayed);

        StandingRow second = rows[1];
        Assert.Equal(2, second.Rank);
        Assert.Equal(teamB, second.TeamId);
        Assert.Equal(40, second.BestScore);
        Assert.Equal(70, second.TotalScore);
        Assert.Equal(2, second.MatchesPlayed);
    }

    private static MissionValueDto Bool(string id, string raw) => new(id, 0, "boolean", raw);
    private static MissionValueDto Number(string id, string raw) => new(id, 0, "number", raw);
    private static MissionValueDto Enum(string id, string raw) => new(id, 0, "enum", raw);

    /// <summary>Creates one ranking match for the round, then scores+submits both teams' sheets.</summary>
    private static async Task PlayRound(
        HttpClient c, DivisionGraph g, int round,
        List<MissionValueDto> aMissions, List<MissionValueDto> bMissions)
    {
        var req = new MatchRequest(round, 1, "Ranking",
        [
            new MatchParticipantRequest(g.TableIds[0], g.TeamIds[0]),
            new MatchParticipantRequest(g.TableIds[1], g.TeamIds[1]),
        ]);
        MatchResponse match = await (await c.PostAsJsonAsync(
            $"/divisions/{g.DivisionId}/matches", req, TestApi.Json)).ReadAsync<MatchResponse>();

        await Score(c, ParticipantOf(match, g.TeamIds[0]), aMissions);
        await Score(c, ParticipantOf(match, g.TeamIds[1]), bMissions);
    }

    private static Guid ParticipantOf(MatchResponse match, Guid teamId) =>
        match.Participants.Single(p => p.TeamId == teamId).Id;

    private static async Task Score(HttpClient c, Guid participantId, List<MissionValueDto> missions)
    {
        await c.PutAsJsonAsync($"/scoresheets/{participantId}",
            new ScoresheetUpdateRequest(missions), TestApi.Json);
        (await c.PostAsync($"/scoresheets/{participantId}/submit", null)).EnsureSuccessStatusCode();
    }
}
