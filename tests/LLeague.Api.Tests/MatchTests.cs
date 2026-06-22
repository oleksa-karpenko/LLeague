using System.Net;
using System.Net.Http.Json;
using LLeague.Api.Application;
using LLeague.Api.Tests.Infrastructure;
using Xunit;

namespace LLeague.Api.Tests;

/// <summary>
/// Locks the match-creation validation rules and the start/complete/abort state
/// machine that currently live in MatchesController.
/// </summary>
[Collection(ApiCollection.Name)]
public class MatchTests(PostgresFixture fixture)
{
    private static MatchRequest RankingMatch(DivisionGraph g, int round = 1, int number = 1) =>
        new(round, number, "Ranking",
        [
            new MatchParticipantRequest(g.TableIds[0], g.TeamIds[0]),
            new MatchParticipantRequest(g.TableIds[1], g.TeamIds[1]),
        ]);

    [Fact]
    public async Task Create_match_with_valid_participants_succeeds()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();

        HttpResponseMessage res = await c.PostAsJsonAsync(
            $"/divisions/{g.DivisionId}/matches", RankingMatch(g), TestApi.Json);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        MatchResponse match = await res.ReadAsync<MatchResponse>();
        Assert.Equal(2, match.Participants.Count);
        Assert.Equal("NotStarted", match.Status);
    }

    [Fact]
    public async Task Create_match_with_unknown_stage_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();

        var req = new MatchRequest(1, 1, "Nonsense",
            [new MatchParticipantRequest(g.TableIds[0], g.TeamIds[0])]);
        HttpResponseMessage res = await c.PostAsJsonAsync($"/divisions/{g.DivisionId}/matches", req, TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_match_with_no_participants_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();

        var req = new MatchRequest(1, 1, "Ranking", []);
        HttpResponseMessage res = await c.PostAsJsonAsync($"/divisions/{g.DivisionId}/matches", req, TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_match_with_table_from_another_division_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();
        DivisionGraph other = await c.CreateDivisionGraphAsync();

        var req = new MatchRequest(1, 1, "Ranking",
            [new MatchParticipantRequest(other.TableIds[0], g.TeamIds[0])]);
        HttpResponseMessage res = await c.PostAsJsonAsync($"/divisions/{g.DivisionId}/matches", req, TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_match_with_unenrolled_team_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();
        Guid strangerTeam = await c.CreateTeamAsync();   // exists but not enrolled here

        var req = new MatchRequest(1, 1, "Ranking",
            [new MatchParticipantRequest(g.TableIds[0], strangerTeam)]);
        HttpResponseMessage res = await c.PostAsJsonAsync($"/divisions/{g.DivisionId}/matches", req, TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Create_match_reusing_a_table_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();

        var req = new MatchRequest(1, 1, "Ranking",
        [
            new MatchParticipantRequest(g.TableIds[0], g.TeamIds[0]),
            new MatchParticipantRequest(g.TableIds[0], g.TeamIds[1]),   // same table twice
        ]);
        HttpResponseMessage res = await c.PostAsJsonAsync($"/divisions/{g.DivisionId}/matches", req, TestApi.Json);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Match_lifecycle_start_complete_abort()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();
        MatchResponse match = await (await c.PostAsJsonAsync(
            $"/divisions/{g.DivisionId}/matches", RankingMatch(g), TestApi.Json)).ReadAsync<MatchResponse>();

        Assert.Equal(HttpStatusCode.NoContent, (await c.PostAsync($"/matches/{match.Id}/start", null)).StatusCode);
        Assert.Equal("InProgress", await GetStatus(c, g.DivisionId, match.Id));

        Assert.Equal(HttpStatusCode.NoContent, (await c.PostAsync($"/matches/{match.Id}/complete", null)).StatusCode);
        Assert.Equal("Completed", await GetStatus(c, g.DivisionId, match.Id));

        Assert.Equal(HttpStatusCode.NoContent, (await c.PostAsync($"/matches/{match.Id}/abort", null)).StatusCode);
        Assert.Equal("NotStarted", await GetStatus(c, g.DivisionId, match.Id));
    }

    [Fact]
    public async Task Completing_a_not_started_match_is_a_conflict()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        DivisionGraph g = await c.CreateDivisionGraphAsync();
        MatchResponse match = await (await c.PostAsJsonAsync(
            $"/divisions/{g.DivisionId}/matches", RankingMatch(g), TestApi.Json)).ReadAsync<MatchResponse>();

        HttpResponseMessage res = await c.PostAsync($"/matches/{match.Id}/complete", null);

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    private static async Task<string> GetStatus(HttpClient c, Guid divisionId, Guid matchId)
    {
        List<MatchResponse> matches = await (await c.GetAsync($"/divisions/{divisionId}/matches"))
            .ReadAsync<List<MatchResponse>>();
        return matches.Single(m => m.Id == matchId).Status;
    }
}
