## What & why

<!-- What does this PR change, and why? Link any issue: Closes #123 -->

## Changes

-

## Checklist

- [ ] Tests added/updated (backend xUnit / frontend Vitest)
- [ ] `cd backend && ./coverage.sh` passes locally (≥ 60% line coverage); build is warning-free
- [ ] `cd backend && dotnet format whitespace LLeague.slnx --verify-no-changes` is clean
- [ ] `cd frontend && npm run lint && npm run typecheck && npm run format:check && npm run test` are clean
- [ ] `cd frontend && npm run build` is clean
- [ ] No secrets committed (`.env`, real `appsettings.json`, JWT/passwords)
- [ ] README / docs updated if behavior or setup changed
