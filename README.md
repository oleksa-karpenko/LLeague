# LLeague

[![CI](https://github.com/oleksa-karpenko/LLeague/actions/workflows/ci.yml/badge.svg)](https://github.com/oleksa-karpenko/LLeague/actions/workflows/ci.yml)
[![coverage](https://oleksa-karpenko.github.io/LLeague/badge_linecoverage.svg)](https://oleksa-karpenko.github.io/LLeague/)

A full-stack app for running **robotics tournaments** — managing seasons, events, divisions,
teams, match scheduling, server-authoritative scoring, and live standings.

Built as a study in pragmatic **Clean Architecture** on **.NET 10**, with a **React + TypeScript**
front end, **PostgreSQL**, real integration tests (Testcontainers), and Docker.

## Tech stack

| Layer | Tech |
|---|---|
| API | ASP.NET Core 10 (controllers), EF Core 10 + Npgsql, JWT auth, BCrypt |
| Frontend | React 19, TypeScript, Vite, TanStack Query, React Router |
| Database | PostgreSQL 17 |
| Tests | xUnit, Testcontainers for PostgreSQL, coverlet |
| Ops | Docker / docker-compose, nginx (SPA), GitHub Actions CI |

## Architecture

The API is a single project organized by **Clean Architecture layers**, with dependencies
pointing inward only:

```
Controllers ──► Application ──► Domain
     │              ▲
     └► Infrastructure ────────┘
```

- **Domain** — entities with behavior (`Match` state machine, `Scoresheet`, `StandingsCalculator`),
  the scoring rules (`ScoringService`, `MissionCatalog`), and domain exceptions. No external deps.
- **Application** — use-case services (`MatchService`, `ScoresheetService`, …), DTOs, and the
  `IAppDbContext` persistence seam. Depends only on Domain.
- **Infrastructure** — EF `AppDbContext`, migrations, JWT/BCrypt implementations. Implements the
  Application abstractions.
- **Controllers / Program** — thin HTTP layer; `ExceptionHandlingMiddleware` maps domain
  exceptions to status codes (404/401/409/400).

EF Core is the unit of work (no repository wrappers). See [`coverage.md`](backend/coverage.md) for the
test-coverage workflow.

## How scoring works (the domain in one paragraph)

An admin creates a **season → event → division**, registers **teams** and enrolls them, adds
**tables**, then schedules **matches**. Each match participant gets a **scoresheet**; the server
computes the score from a **mission catalog** (boolean / counted / multiple-choice clauses) — the
client is never trusted. **Standings** rank teams by their best single match score, then total,
then team number. Only completed *ranking*-stage scoresheets count.

## Getting started

### Option A — Docker (everything containerized)

```bash
cp .env.example .env        # then edit .env with your own secrets
docker compose up --build   # or: ./ll.sh up --build
```

- Web: http://localhost:5173
- API: http://localhost:8080
- Sign in with `admin` / the `ADMIN_PASSWORD` you set in `.env`.

### Option B — Local dev (hot reload)

Requires the .NET 10 SDK, Node 20+, and Docker (for PostgreSQL).

```bash
cp backend/src/LLeague.Api/appsettings.Development.json.example backend/src/LLeague.Api/appsettings.Development.json   # then edit it
./ll.sh dev     # Postgres in Docker + API (dotnet) + web (vite), all hot-reloading
```

Run `./ll.sh help` for the full command list (db, migrate, logs, dockerclean, …).

## Configuration & secrets

Secrets are **never committed**. Provide them via the git-ignored files:

| File | Used by | Template |
|---|---|---|
| `.env` | docker-compose | `.env.example` |
| `backend/src/LLeague.Api/appsettings.Development.json` | local `dotnet run` | `…/appsettings.Development.json.example` |

Required keys: `POSTGRES_PASSWORD`, `JWT_SECRET` (≥ 32 chars), `ADMIN_PASSWORD`.

## Testing

```bash
cd backend
dotnet test LLeague.slnx        # 43 tests: domain unit + API integration (Testcontainers)
./coverage.sh                   # run tests + open an HTML coverage report
```

Docker must be running — the integration tests spin up a throwaway PostgreSQL container.
The pure unit tests don't need it: `dotnet test --filter "FullyQualifiedName~ScoringServiceTests"`.

The frontend uses **Vitest + React Testing Library** (`cd frontend && npm run test`).

## Quality gates & developer tooling

All gates are enforced in CI and block PRs. Run them locally before pushing:

```bash
# Backend  (build treats compiler + analyzer warnings as errors — see Directory.Build.props)
cd backend
dotnet format whitespace LLeague.slnx --verify-no-changes   # formatting gate
./coverage.sh                                                # tests + 60% line-coverage gate

# Frontend
cd frontend
npm run lint          # ESLint (warnings fail)
npm run typecheck     # tsc
npm run format:check  # Prettier
npm run test          # Vitest + React Testing Library
```

**Pre-commit hooks** (husky + lint-staged): run `npm install` once at the **repo root** to enable
them. On commit, staged frontend files are auto-fixed (ESLint + Prettier) and staged C# is
whitespace-formatted; a pre-push hook runs the frontend typecheck + tests.

**AI assistant config**: conventions for GitHub Copilot / Claude Code live in
`.github/copilot-instructions.md`, path-scoped `.github/instructions/*.instructions.md`,
`AGENTS.md`, and reusable prompts under `.github/prompts/`. The full plan and a measurement loop
are in [`Copilot.md`](Copilot.md); track config experiments in
[`docs/ai-config-log.md`](docs/ai-config-log.md) with `scripts/ai-metrics.sh`.

## Project structure

```
backend/                      all .NET lives here, self-contained
  LLeague.slnx                solution (spans the API + tests)
  Directory.Build.props       shared MSBuild settings (artifacts output)
  nuget.config
  coverage.sh / coverage.md   test-coverage workflow
  src/LLeague.Api/            ASP.NET Core API (Domain / Application / Infrastructure / Controllers)
  tests/LLeague.Api.Tests/    xUnit tests (unit + Testcontainers integration)
frontend/                     React + Vite SPA (all web tooling lives here)
docker-compose.yml            full-stack orchestration (db + api + web)
ll.sh                         dev/ops helper script
```

## Known trade-offs

- **CORS** is wide open in development (`AllowAnyOrigin`) — restrict it before any real deployment.
- The SPA stores the JWT in `localStorage` (simple, but XSS-exposed) — fine for an internal admin
  tool, reconsider for untrusted contexts.

## License

[MIT](LICENSE) © Oleksandr Karpenko
