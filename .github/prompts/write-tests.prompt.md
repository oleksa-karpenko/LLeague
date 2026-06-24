---
mode: agent
description: Generate tests for a given file (xUnit backend or Vitest frontend)
---

Write tests for the file I point you at (or the current selection).

**If it's backend C# (`backend/...`):** use xUnit.
- Pure domain logic (e.g. `ScoringService`) → fast unit tests, no Docker.
- Controllers / services exercised over HTTP → integration tests with `[Collection(ApiCollection.Name)]`,
  `PostgresFixture`, and `fixture.Factory.CreateClient()`. Use the `TestApi` helpers.
- Cover the happy path **and** each domain-exception path. Follow `.github/instructions/backend-tests.instructions.md`.

**If it's frontend TS/TSX (`frontend/...`):** use Vitest + React Testing Library.
- Query by role/text, drive with `@testing-library/user-event`, assert on user-visible behavior.
- Mock the `src/api/` boundary. Follow `.github/instructions/frontend-tests.instructions.md`.

Aim for meaningful coverage of branches and error handling, not just the happy path. Then run the
relevant test command and confirm the new tests pass.
