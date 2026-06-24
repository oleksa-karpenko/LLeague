# LLeague — Copilot instructions

LLeague is a tournament-control app for FLL-style robotics leagues: a **.NET 10 / ASP.NET Core API**
backend and a **React 19 + Vite SPA** frontend, backed by **PostgreSQL** and EF Core.

These are repo-wide instructions; path-specific rules live in `.github/instructions/*.instructions.md`
and are applied automatically by `applyTo` globs. The same conventions are in `AGENTS.md` / `CLAUDE.md`.

## Architecture (backend = Clean Architecture)

```
backend/src/LLeague.Api/
  Domain/         entities, enums, ScoringService, Exceptions/  (no external deps)
  Application/    services (Services/), DTOs (*Dtos.cs), Abstractions/ (IAppDbContext, ITokenService, IPasswordHasher)
  Infrastructure/ AppDbContext, EF Migrations, JWT + BCrypt impls, DbSeeder
  Controllers/    thin HTTP layer; delegate to Application services
  Middleware/     ExceptionHandlingMiddleware maps domain exceptions -> HTTP status
frontend/src/
  api/            client.ts (fetch wrapper + ApiError), endpoints.ts, types.ts
  auth/           ProtectedRoute
  components/     shared UI (Layout, ui.tsx, DivisionNav)
  pages/          route screens
```

Dependency rule: `Domain` ← `Application` ← `Infrastructure`/`Controllers`. Never make Domain or
Application depend on EF/ASP.NET concretions — go through `IAppDbContext` and the other abstractions.

## Golden rules

- **Keep controllers thin** — no business logic; call an Application service and return its result.
- **Business logic lives in Application/Domain.** Scoring rules belong in `Domain/ScoringService`.
- **Throw domain exceptions** (`ValidationException`, `NotFoundException`, `UnauthorizedException`,
  `ConflictException`) — the middleware turns them into the right HTTP status. Don't return raw error codes.
- **Respect `.editorconfig`** (it's enforced in the build) and existing patterns: primary constructors,
  file-scoped namespaces, `record` DTOs, nullable reference types.
- **Never commit secrets** — `.env`, real `appsettings.json`, JWT secrets, passwords stay out of git.
- **Tests + gates must pass.** Definition of done = formatted, lint-clean, tests green, coverage gate met.

## Commands

```bash
./ll.sh dev                         # run API + web + Postgres with hot reload
cd backend && ./coverage.sh         # build, test, enforce 60% line-coverage gate
cd backend && dotnet format LLeague.slnx          # auto-format C#
cd frontend && npm run lint         # ESLint (warnings fail)
cd frontend && npm run typecheck    # tsc --noEmit
cd frontend && npm run format:check # Prettier check
cd frontend && npm run test         # Vitest
```

## Tech versions

.NET 10 · EF Core 10 (Npgsql) · ASP.NET Core 10 · JWT + BCrypt · xUnit + Testcontainers ·
React 19 · TypeScript 6 · Vite 8 · React Router 7 · TanStack Query 5 · PostgreSQL 17.
