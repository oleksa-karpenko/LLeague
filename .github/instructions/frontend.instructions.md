---
applyTo: "frontend/**"
---

# Frontend (React 19 + TypeScript + Vite) conventions

## Structure

- `src/api/` — `client.ts` (the `api<T>()` fetch wrapper + `ApiError` + token helpers),
  `endpoints.ts` (typed endpoint functions), `types.ts` (shared DTO types mirroring the API).
- `src/auth/` — `ProtectedRoute`.
- `src/components/` — shared UI (`Layout`, `ui.tsx`, `DivisionNav`).
- `src/pages/` — one default-exported component per route.

## Code style (`.editorconfig` + Prettier + ESLint, all enforced)

- 2-space indent, **single quotes**, **semicolons**, function components only.
- `erasableSyntaxOnly` is on — **do not** use TS features that emit runtime code:
  no parameter properties (`constructor(public x)`), no non-`const` `enum`, no `namespace`.
  Declare class fields explicitly and assign in the body (see `ApiError` in `api/client.ts`).
- Use `type`-only imports where applicable (`import { useState, type FormEvent } from 'react'`).

## Patterns to follow

- **Server state via TanStack Query** (`useQuery` / `useMutation`); the `QueryClientProvider` is set
  up in `main.tsx`. Don't hand-roll fetch-in-`useEffect` for data that Query should own.
- **All HTTP goes through `src/api/`** — call functions from `endpoints.ts`, which use `api<T>()`.
  Never call `fetch` directly from a component. Handle failures by catching `ApiError` (`err.message`).
- **Auth**: JWT is stored in `localStorage` via the helpers in `client.ts`; guard routes with `ProtectedRoute`.
- Reuse the shared primitives in `components/ui.tsx` (`ErrorBanner`, `Spinner`, icons, `.btn`/`.card` classes).
- Local component state with `useState`; keep pages as the composition layer, push reusable bits into `components/`.

## Before you finish

```bash
cd frontend
npm run lint          # ESLint, warnings fail
npm run typecheck     # tsc --noEmit
npm run format:check  # Prettier
npm run test          # Vitest
```

Add a colocated `*.test.tsx` for new components/logic — see `.github/instructions/frontend-tests.instructions.md`.
