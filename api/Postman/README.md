# LLeague2 — Postman collection

`LLeague2.postman_collection.json` covers every endpoint of the tutorial API (Steps 11–17).

## Import
Postman → **Import** → drop in `LLeague2.postman_collection.json`. (Works in Insomnia and the VS Code
/ Rider REST clients that accept Postman v2.1 collections too.)

## Use it
1. Open the collection's **Variables** tab and set **`baseUrl`** to your API (default
   `http://localhost:5084` — check your `dotnet run` console for the actual port).
2. Run **Auth → Login** first. A test script saves the JWT into the **`token`** variable; every other
   request sends `Authorization: Bearer {{token}}` automatically (set at the collection level).
3. The **Create** requests save their new id into a collection variable
   (`seasonId`, `eventId`, `divisionId`, `teamId`, `tableId`, `matchId`, `participantId`), so the
   follow-up Get/Update/Delete and nested requests just work.

## Suggested end-to-end run order
Login → Seasons:Create → Events:Create → Divisions:Create → Teams:Create →
Divisions:Enroll team → Divisions:Create table → Matches:Create → Matches:Start →
Scoresheets:Upsert → Scoresheets:Submit → Matches:Complete → Divisions:Standings.

## Environments
Two ready-made environment files let you switch the target with one dropdown:
- **`LLeague2.local.postman_environment.json`** — `baseUrl = http://localhost:5084` (native `dotnet run`)
- **`LLeague2.docker.postman_environment.json`** — `baseUrl = http://localhost:8080` (Docker Compose, Step 22)

Import them (Postman → Import), then pick one from the environment selector (top-right). Each also
holds `username` / `password`, which the **Login** request now reads (`{{username}}`/`{{password}}`),
so you can change credentials per environment without editing the request.

> The runtime values captured by scripts (`token`, `seasonId`, …) stay as **collection** variables —
> the environments only override `baseUrl`/`username`/`password`, so there's no conflict.

## Notes
- **Folders:** Health, Auth, Seasons, Events, Divisions (incl. enrollment/tables/standings), Teams,
  Matches, Scoresheets.
- **Public endpoints** (no token): `Health`, `Auth → Login`, `Scoresheets → Missions catalog`.
- The **"Create team – INVALID region"** request intentionally expects **400** (demonstrates the
  2-letter region validation).
