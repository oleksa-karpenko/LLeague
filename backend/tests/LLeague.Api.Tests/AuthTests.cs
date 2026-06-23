using System.Net;
using System.Net.Http.Json;
using LLeague.Api.Application;
using LLeague.Api.Tests.Infrastructure;
using Xunit;

namespace LLeague.Api.Tests;

[Collection(ApiCollection.Name)]
public class AuthTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Login_with_seeded_admin_returns_a_token()
    {
        HttpClient client = fixture.Factory.CreateClient();

        HttpResponseMessage res = await client.PostAsJsonAsync(
            "/auth/login", new LoginRequest(TestApi.AdminUserName, TestApi.AdminPassword), TestApi.Json);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        LoginResponse body = await res.ReadAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
    }

    [Fact]
    public async Task Login_with_wrong_password_is_unauthorized()
    {
        HttpClient client = fixture.Factory.CreateClient();

        HttpResponseMessage res = await client.PostAsJsonAsync(
            "/auth/login", new LoginRequest(TestApi.AdminUserName, "wrong"), TestApi.Json);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_without_token_is_unauthorized()
    {
        HttpClient client = fixture.Factory.CreateClient();

        HttpResponseMessage res = await client.GetAsync("/seasons");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
