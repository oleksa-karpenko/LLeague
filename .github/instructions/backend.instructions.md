---
applyTo: "backend/**"
---

# Backend (.NET 10 / ASP.NET Core) conventions

## Architecture

Clean Architecture, one layer depending only inward:

- `Domain/` — entities, enums, `ScoringService`, `Exceptions/`. **No EF, no ASP.NET, no DI.**
- `Application/` — `Services/` (use cases), `*Dtos.cs` (request/response `record`s),
  `Abstractions/` (`IAppDbContext`, `ITokenService`, `IPasswordHasher`).
- `Infrastructure/` — `AppDbContext` (implements `IAppDbContext`), EF `Migrations/`, JWT + BCrypt, `DbSeeder`.
- `Controllers/` — thin HTTP layer.

Never reference EF Core or Infrastructure types from Domain or Application — go through the
abstractions. Persistence uses **EF Core as the unit of work; there are no repositories.**

## Code style (enforced by `.editorconfig` at build time)

- **File-scoped namespaces**, 4-space indent.
- **Primary constructors** for controllers and services:
  `public class AuthService(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)`.
- DTOs are `record`s, defined in `Application/*Dtos.cs` (e.g. `record LoginRequest(string Username, string Password)`).
- `_camelCase` private fields, `I`-prefixed interfaces, nullable reference types enabled.
- Prefer `var` when the type is apparent; expression-bodied properties/accessors OK, not methods.

## Patterns to follow

- **Controllers**: `[ApiController]`, lowercase route (`[Route("auth")]`), one-liners that delegate:
  `return Ok(await service.DoAsync(req));`. Use `[Authorize]` / `[AllowAnonymous]`. No business logic, no try/catch.
- **Validation & errors**: throw domain exceptions — `ValidationException`, `NotFoundException`,
  `UnauthorizedException`, `ConflictException`. `Middleware/ExceptionHandlingMiddleware` maps them to
  HTTP status + the `{ error }` shape the frontend expects. Do **not** return `BadRequest(...)` etc. manually.
- **Data access**: inject `IAppDbContext`; use async EF (`FirstOrDefaultAsync`, `ToListAsync`, `SaveChangesAsync`).
- **Scoring / domain rules** belong in `Domain/ScoringService`, not in services or controllers.
- Register new services in `Application/DependencyInjection.cs` (and infra impls in `Infrastructure/DependencyInjection.cs`).

## EF migrations

When you change an entity or `AppDbContext`:

```bash
cd backend/src/LLeague.Api
dotnet ef migrations add <Name> -o Infrastructure/Migrations
```

Add the matching `DbSet` to **both** `IAppDbContext` and `AppDbContext`. Review the generated migration.

## Before you finish

- `cd backend && dotnet format LLeague.slnx` (build fails on style/analyzer violations — warnings are errors).
- `cd backend && ./coverage.sh` — tests pass and line coverage ≥ 60%.
- Add/extend tests for new behavior (see `.github/instructions/backend-tests.instructions.md`).
