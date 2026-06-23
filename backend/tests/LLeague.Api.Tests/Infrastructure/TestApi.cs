using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LLeague.Api.Application;

namespace LLeague.Api.Tests.Infrastructure;

/// <summary>Auth + data-setup helpers shared by the integration tests.</summary>
public static class TestApi
{
    public const string AdminUserName = "admin";
    public const string AdminPassword = "test-admin-password";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // Unique-ish team numbers per run (the container is fresh each run; teams are
    // unique on (Number, Region), so we only need uniqueness within one run).
    private static int _teamSeq = 9000;
    public static int NextTeamNumber() => Interlocked.Increment(ref _teamSeq);

    public static async Task<HttpClient> CreateAuthedClientAsync(this ApiFactory factory)
    {
        HttpClient client = factory.CreateClient();
        HttpResponseMessage res = await client.PostAsJsonAsync(
            "/auth/login", new LoginRequest(AdminUserName, AdminPassword), Json);
        res.EnsureSuccessStatusCode();
        LoginResponse body = (await res.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return client;
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage res)
    {
        await EnsureOkAsync(res);
        return (await res.Content.ReadFromJsonAsync<T>(Json))!;
    }

    public static async Task EnsureOkAsync(this HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode)
        {
            return;
        }

        string body = await res.Content.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"{(int)res.StatusCode} {res.StatusCode} for {res.RequestMessage?.Method} {res.RequestMessage?.RequestUri}: {body}");
    }

    // ----- Graph builders (each returns the created id) -----

    public static async Task<Guid> CreateSeasonAsync(this HttpClient c)
    {
        SeasonResponse s = await (await c.PostAsJsonAsync(
            "/seasons", new SeasonRequest("Test Season", 2026), Json)).ReadAsync<SeasonResponse>();
        return s.Id;
    }

    public static async Task<Guid> CreateEventAsync(this HttpClient c, Guid seasonId)
    {
        var req = new EventRequest(seasonId, "Test Event", "",
            new DateOnly(2026, 3, 14), new DateOnly(2026, 3, 14), "Somewhere");
        EventResponse e = await (await c.PostAsJsonAsync("/events", req, Json)).ReadAsync<EventResponse>();
        return e.Id;
    }

    public static async Task<Guid> CreateDivisionAsync(this HttpClient c, Guid eventId)
    {
        DivisionResponse d = await (await c.PostAsJsonAsync(
            "/divisions", new DivisionRequest(eventId, "Test Division", "#1f6feb"), Json))
            .ReadAsync<DivisionResponse>();
        return d.Id;
    }

    public static async Task<Guid> CreateTableAsync(this HttpClient c, Guid divisionId, string name)
    {
        TableResponse t = await (await c.PostAsJsonAsync(
            $"/divisions/{divisionId}/tables", new TableRequest(name), Json)).ReadAsync<TableResponse>();
        return t.Id;
    }

    public static async Task<Guid> CreateTeamAsync(this HttpClient c, int? number = null)
    {
        int n = number ?? NextTeamNumber();
        TeamResponse t = await (await c.PostAsJsonAsync(
            "/teams", new TeamRequest(n, $"Team {n}", "Some School", "Some City", "IL"), Json))
            .ReadAsync<TeamResponse>();
        return t.Id;
    }

    public static async Task EnrollAsync(this HttpClient c, Guid divisionId, Guid teamId)
    {
        HttpResponseMessage res = await c.PostAsJsonAsync(
            $"/divisions/{divisionId}/teams", new EnrollRequest(teamId), Json);
        await res.EnsureOkAsync();
    }

    /// <summary>Builds a division with <paramref name="count"/> tables and the same number of enrolled teams.</summary>
    public static async Task<DivisionGraph> CreateDivisionGraphAsync(this HttpClient c, int count = 2)
    {
        Guid seasonId = await c.CreateSeasonAsync();
        Guid eventId = await c.CreateEventAsync(seasonId);
        Guid divisionId = await c.CreateDivisionAsync(eventId);

        var tables = new List<Guid>();
        var teams = new List<Guid>();
        for (int i = 0; i < count; i++)
        {
            tables.Add(await c.CreateTableAsync(divisionId, $"Table {i + 1}"));
            Guid teamId = await c.CreateTeamAsync();
            await c.EnrollAsync(divisionId, teamId);
            teams.Add(teamId);
        }

        return new DivisionGraph(eventId, divisionId, tables, teams);
    }
}

public sealed record DivisionGraph(Guid EventId, Guid DivisionId, List<Guid> TableIds, List<Guid> TeamIds);
