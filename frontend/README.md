# LLeague — Web (frontend)

React 19 + TypeScript + Vite SPA for the LLeague API. See the [root README](../README.md)
for the full project overview, architecture, and Docker setup.

## Develop

```bash
npm install
npm run dev      # Vite dev server on http://localhost:5173
```

The API base URL comes from `VITE_API_BASE` (defaults to `http://localhost:8080`).
Easiest path for the whole stack is `./ll.sh dev` from the repo root (API + web + DB, hot reload).

## Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start the Vite dev server (HMR) |
| `npm run build` | Type-check and build for production |
| `npm run preview` | Preview the production build locally |
| `npm run lint` | Run ESLint |

## Structure

```
src/
  api/         typed fetch client + endpoints + DTO types
  auth/        ProtectedRoute (JWT gate)
  components/  layout + shared UI
  pages/       Seasons, Events, Divisions, Teams, Setup, Score, Board, Login
```

Production builds are served by nginx (see `Dockerfile` and `nginx.conf`).
