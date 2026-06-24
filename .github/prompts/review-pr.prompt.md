---
mode: agent
description: Review the current changes against LLeague conventions and the PR checklist
---

Review the current diff (staged/working changes, or the PR if one is open) against this project's
conventions. Report findings grouped by severity (blocker / should-fix / nit), each with file:line.

Check for:

- **Architecture**: layering respected (Domain/Application free of EF/ASP.NET); controllers thin;
  business logic in services/Domain, not controllers.
- **Error handling**: domain exceptions thrown (not manual status codes); middleware contract intact.
- **Style**: matches `.editorconfig` (file-scoped namespaces, primary constructors, `record` DTOs,
  `_camelCase` fields); frontend respects `erasableSyntaxOnly`, single quotes, TanStack Query usage,
  all HTTP via `src/api/`.
- **Tests**: new behavior is tested (happy + error paths); coverage gate (≥60% backend) not regressed.
- **Security**: no secrets committed; auth attributes correct; input validated.
- **Gates**: would `dotnet format --verify-no-changes`, `npm run lint/typecheck/format:check`, and
  the test suites pass?

Don't rewrite the code — list precise, actionable findings I can act on.
