---
applyTo: "backend/tests/**"
---

# Backend tests (xUnit + Testcontainers)

Tests live in `backend/tests/LLeague.Api.Tests/`. Split by nature:

- **Unit tests** (`Domain/`) — pure, no I/O, no Docker. Test `ScoringService` and domain rules directly.
- **Integration tests** — drive the real API over HTTP through `WebApplicationFactory` against a
  throwaway **Postgres Testcontainer**. These need Docker running locally.

## Integration-test pattern

- Put the class in the shared collection so the Postgres container/factory is reused:
  `[Collection(ApiCollection.Name)]` and take `PostgresFixture fixture` via primary constructor.
- Get a client with `fixture.Factory.CreateClient()`.
- Use the `TestApi` helpers: `TestApi.AdminUserName` / `TestApi.AdminPassword` / `TestApi.Json`,
  and the `ReadAsync<T>()` extension for response bodies.
- Assert on HTTP status **and** body. Example:

```csharp
[Collection(ApiCollection.Name)]
public class FooTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Endpoint_does_the_thing()
    {
        HttpClient client = fixture.Factory.CreateClient();
        HttpResponseMessage res = await client.GetAsync("/foo");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
```

## Conventions

- xUnit (`[Fact]` / `[Theory]`), Arrange-Act-Assert, descriptive `snake_case` method names
  (`Login_with_wrong_password_is_unauthorized`).
- For auth-protected endpoints, obtain a token via `/auth/login` first, then set the `Authorization` header.
- Cover the happy path **and** the domain-exception paths (validation/not-found/unauthorized/conflict).

## Running

```bash
cd backend && ./coverage.sh    # runs all tests + enforces the 60% line-coverage gate
dotnet test LLeague.slnx       # quick run without the coverage gate
```

New behavior must keep line coverage ≥ 60%.
