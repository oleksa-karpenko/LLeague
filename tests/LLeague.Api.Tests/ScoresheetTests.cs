using System.Net;
using System.Net.Http.Json;
using LLeague.Api.Application;
using LLeague.Api.Tests.Infrastructure;
using Xunit;

namespace LLeague.Api.Tests;

/// <summary>
/// Locks the scoresheet upsert/submit workflow: the score is computed
/// server-side and status moves Empty -> Draft -> Completed.
/// </summary>
[Collection(ApiCollection.Name)]
public class ScoresheetTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Unscored_participant_reports_empty_sheet()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        Guid participantId = await CreateParticipant(c);

        ScoresheetResponse sheet = await (await c.GetAsync($"/scoresheets/{participantId}"))
            .ReadAsync<ScoresheetResponse>();

        Assert.Equal("Empty", sheet.Status);
        Assert.Equal(0, sheet.Score);
        Assert.Empty(sheet.Missions);
    }

    [Fact]
    public async Task Upsert_computes_score_server_side_and_drafts()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        Guid participantId = await CreateParticipant(c);

        var req = new ScoresheetUpdateRequest(
        [
            new MissionValueDto("M01", 0, "boolean", "true"),  // 10
            new MissionValueDto("M02", 0, "number", "6"),      // 30
        ]);
        ScoresheetResponse sheet = await (await c.PutAsJsonAsync(
            $"/scoresheets/{participantId}", req, TestApi.Json)).ReadAsync<ScoresheetResponse>();

        Assert.Equal("Draft", sheet.Status);
        Assert.Equal(40, sheet.Score);
        Assert.Equal(2, sheet.Missions.Count);
    }

    [Fact]
    public async Task Submit_finalizes_the_sheet()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        Guid participantId = await CreateParticipant(c);
        await c.PutAsJsonAsync($"/scoresheets/{participantId}",
            new ScoresheetUpdateRequest([new MissionValueDto("M01", 0, "boolean", "true")]), TestApi.Json);

        ScoresheetResponse sheet = await (await c.PostAsync($"/scoresheets/{participantId}/submit", null))
            .ReadAsync<ScoresheetResponse>();

        Assert.Equal("Completed", sheet.Status);
        Assert.Equal(10, sheet.Score);
    }

    [Fact]
    public async Task Submitting_without_a_sheet_is_bad_request()
    {
        HttpClient c = await fixture.Factory.CreateAuthedClientAsync();
        Guid participantId = await CreateParticipant(c);

        HttpResponseMessage res = await c.PostAsync($"/scoresheets/{participantId}/submit", null);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    /// <summary>Creates a single-participant match and returns its participant id.</summary>
    private static async Task<Guid> CreateParticipant(HttpClient c)
    {
        DivisionGraph g = await c.CreateDivisionGraphAsync(count: 1);
        var req = new MatchRequest(1, 1, "Ranking",
            [new MatchParticipantRequest(g.TableIds[0], g.TeamIds[0])]);
        MatchResponse match = await (await c.PostAsJsonAsync(
            $"/divisions/{g.DivisionId}/matches", req, TestApi.Json)).ReadAsync<MatchResponse>();
        return match.Participants[0].Id;
    }
}
