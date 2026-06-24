---
mode: agent
description: Create a new React page/component following LLeague frontend conventions
---

Create a new React component or page in the frontend.

Ask me whether it's a **page** (route screen → `src/pages/`) or a reusable **component** (`src/components/`).

Then:

1. Function component, TypeScript, single quotes + semicolons, 2-space indent. Default export for pages.
2. **Data**: use TanStack Query (`useQuery`/`useMutation`) for server state; call typed functions from
   `src/api/endpoints.ts` (never `fetch` directly). Catch `ApiError` for error display.
3. Reuse shared primitives from `src/components/ui.tsx` (`ErrorBanner`, `Spinner`, `.btn`/`.card`/`.input`
   classes) and `Layout` where appropriate.
4. Respect `erasableSyntaxOnly`: no parameter properties, no runtime `enum`, no `namespace`.
5. If it's a page, wire the route in `src/App.tsx` (guard with `ProtectedRoute` if admin-only).
6. Add a colocated `*.test.tsx` (Vitest + RTL) covering the main interaction —
   see `.github/instructions/frontend-tests.instructions.md`.

Finally: `cd frontend && npm run lint && npm run typecheck && npm run format:check && npm run test`.
