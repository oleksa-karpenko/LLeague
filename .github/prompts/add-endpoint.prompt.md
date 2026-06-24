---
mode: agent
description: Scaffold a new API endpoint following LLeague Clean Architecture
---

Add a new API endpoint to the backend, respecting the Clean Architecture layering.

Ask me for (or infer from context): the route, HTTP verb, request/response shape, and auth requirement.

Then implement, in this order:

1. **DTOs** — add request/response `record`s to the appropriate `Application/*Dtos.cs`.
2. **Application service** — add the use-case method to the relevant `Application/Services/*Service.cs`
   (or create a new service + register it in `Application/DependencyInjection.cs`). Inject `IAppDbContext`.
   Throw domain exceptions (`ValidationException`, `NotFoundException`, `UnauthorizedException`,
   `ConflictException`) for error cases — never return status codes from the service.
3. **Controller action** — add a thin action to the matching `Controllers/*Controller.cs`: correct
   verb/route attribute, `[Authorize]`/`[AllowAnonymous]`, and a one-line delegation to the service.
   No business logic, no try/catch (the middleware handles exceptions).
4. **Tests** — add xUnit integration tests (`[Collection(ApiCollection.Name)]`, `PostgresFixture`)
   covering the happy path and the domain-exception paths. See `.github/instructions/backend-tests.instructions.md`.

Finally run `cd backend && dotnet format LLeague.slnx && ./coverage.sh` and confirm it's green.
