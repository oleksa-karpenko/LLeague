# AGENTS.md — LLeague

Primary agent instructions for this repo. Shared by GitHub Copilot, Claude Code, and other agents.
Deep, path-scoped detail lives in `.github/instructions/*.instructions.md`; the full plan that
introduced this tooling is in `Copilot.md`.

## What this is

LLeague = tournament control for FLL-style robotics leagues.
**Backend:** .NET 10 / ASP.NET Core, EF Core 10, PostgreSQL, Clean Architecture.
**Frontend:** React 19 + TypeScript + Vite SPA, TanStack Query, React Router.

## Layout

- `backend/src/LLeague.Api/` — `Domain/` → `Application/` → `Infrastructure/` + `Controllers/` (see below)
- `backend/tests/LLeague.Api.Tests/` — xUnit + Testcontainers
- `frontend/src/` — `api/`, `auth/`, `components/`, `pages/`
- `Copilot.md`, `docs/ai-config-log.md`, `scripts/ai-metrics.sh` — tooling/process

## Golden rules

1. **Clean Architecture layering**: `Domain ← Application ← Infrastructure/Controllers`. Domain &
   Application never depend on EF/ASP.NET — use `IAppDbContext` and the other abstractions.
2. **Controllers are thin** — delegate to an Application service, return its result.
3. **Throw domain exceptions** (`ValidationException`, `NotFoundException`, `UnauthorizedException`,
   `ConflictException`); the middleware maps them to HTTP. Don't hand-roll status codes.
4. **Match existing style**: primary constructors, file-scoped namespaces, `record` DTOs, nullable
   enabled. `.editorconfig` is enforced at build time.
5. **No secrets in git** (`.env`, real `appsettings.json`, JWT secret, passwords).

## Commands

```bash
./ll.sh dev                                  # API + web + Postgres, hot reload
cd backend && ./coverage.sh                  # test + 60% coverage gate
cd backend && dotnet format LLeague.slnx     # format C#
cd frontend && npm run lint && npm run typecheck && npm run format:check && npm run test
```

## Definition of done

- `dotnet format --verify-no-changes` clean, no new analyzer warnings (build treats them as errors).
- Frontend `lint` + `typecheck` + `format:check` clean.
- Tests added/updated and green; backend line coverage ≥ 60%.
- No secrets committed; docs updated if behavior or setup changed.
