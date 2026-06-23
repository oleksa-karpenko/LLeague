#!/usr/bin/env bash
#
# lleague.sh — manage & run the LLeague2 full-stack solution (db + api + web).
#
# Two ways to run:
#   • Containerized  : everything in Docker (production-like).  -> ./lleague.sh up
#   • Dev / hot-reload: Postgres in Docker, api via `dotnet run`,
#                       web via `npm run dev`. Edit code, see it live. -> ./lleague.sh dev
#
# Run `./lleague.sh help` for the full command list.

set -euo pipefail

# --- Paths ---------------------------------------------------------------
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$ROOT/backend/src/LLeague.Api"
WEB_DIR="$ROOT/frontend"
DB_CONTAINER="lleague2-db"

# --- URLs / ports --------------------------------------------------------
WEB_DOCKER_URL="http://127.0.0.1:5173"   # web container (nginx)
API_DOCKER_URL="http://127.0.0.1:8080"   # api container
WEB_DEV_URL="http://127.0.0.1:5173"      # vite dev server
API_DEV_URL="http://127.0.0.1:5084"      # dotnet run (launchSettings 'http')

# --- Colors --------------------------------------------------------------
if [[ -t 1 ]]; then
  C_BLUE=$'\033[34m'; C_GREEN=$'\033[32m'; C_YELLOW=$'\033[33m'
  C_RED=$'\033[31m'; C_BOLD=$'\033[1m'; C_DIM=$'\033[2m'; C_OFF=$'\033[0m'
else
  C_BLUE=; C_GREEN=; C_YELLOW=; C_RED=; C_BOLD=; C_DIM=; C_OFF=
fi
info()  { printf '%s==>%s %s\n' "$C_BLUE"  "$C_OFF" "$*"; }
ok()    { printf '%s✓%s %s\n'   "$C_GREEN" "$C_OFF" "$*"; }
warn()  { printf '%s!%s %s\n'   "$C_YELLOW" "$C_OFF" "$*"; }
die()   { printf '%s✗%s %s\n'   "$C_RED"   "$C_OFF" "$*" >&2; exit 1; }

# --- Tooling detection ---------------------------------------------------
need() { command -v "$1" >/dev/null 2>&1 || die "'$1' is required but not found in PATH."; }

compose() {
  if docker compose version >/dev/null 2>&1; then
    docker compose "$@"
  elif command -v docker-compose >/dev/null 2>&1; then
    docker-compose "$@"
  else
    die "Neither 'docker compose' nor 'docker-compose' is available."
  fi
}

# Wait until the Postgres container reports ready (api retries too, but this is nicer).
wait_for_db() {
  info "Waiting for Postgres to be ready..."
  for _ in $(seq 1 30); do
    if docker exec "$DB_CONTAINER" pg_isready -U postgres -d lleague >/dev/null 2>&1; then
      ok "Postgres is ready."
      return 0
    fi
    sleep 1
  done
  die "Postgres did not become ready in time. Check: ./lleague.sh logs db"
}

open_url() {
  command -v open       >/dev/null 2>&1 && { open "$1";       return; }
  command -v xdg-open   >/dev/null 2>&1 && { xdg-open "$1";   return; }
  warn "Open manually: $1"
}

# =========================================================================
# Containerized stack (docker compose)
# =========================================================================
cmd_up() {       # ./lleague.sh up [--build]
  need docker
  cd "$ROOT"
  info "Starting full stack (db + api + web) in Docker..."
  compose up -d "$@"
  ok "Stack is up."
  printf '   Web : %s\n   API : %s\n' "$WEB_DOCKER_URL" "$API_DOCKER_URL"
  printf '   %sLogin with your configured admin credentials  •  logs: ./lleague.sh logs%s\n' "$C_DIM" "$C_OFF"
}

cmd_build() {    # ./lleague.sh build [svc...]  — (re)build images only
  need docker; cd "$ROOT"
  info "Building images${*:+ for: $*}..."
  compose build "$@"
  ok "Build complete."
}

cmd_rebuild() {  # ./lleague.sh rebuild [svc...] — rebuild + recreate running containers
  need docker; cd "$ROOT"
  info "Rebuilding & recreating${*:+: $*}..."
  compose up -d --build "$@"
  ok "Rebuilt and running. ($WEB_DOCKER_URL)"
}

cmd_down() {     # ./lleague.sh down — stop stack, KEEP db data
  need docker; cd "$ROOT"
  info "Stopping stack (db volume preserved)..."
  compose down
  ok "Stopped."
}

cmd_reset() {    # ./lleague.sh reset — stop stack and WIPE db volume
  need docker; cd "$ROOT"
  warn "This DESTROYS the database volume (all data)."
  read -r -p "Type 'yes' to continue: " ans
  [[ "$ans" == "yes" ]] || { info "Aborted."; return; }
  compose down -v
  ok "Stack down and db volume wiped."
}

cmd_dockerclean() {  # ./lleague.sh dockerclean [--volumes] — remove project containers (+ stray lleague*) for a fresh 'up'
  need docker; cd "$ROOT"
  local wipe=""
  if [[ "${1:-}" == "--volumes" || "${1:-}" == "-v" ]]; then
    warn "This will also DELETE the database volume (all data)."
    read -r -p "Type 'yes' to continue: " ans
    [[ "$ans" == "yes" ]] || { info "Aborted."; return; }
    wipe="-v"
  fi
  info "Stopping & removing compose containers (orphans included)${wipe:+ + volumes}..."
  compose down --remove-orphans $wipe
  # Sweep up stray containers from older runs / renamed versions (e.g. lleague-db, lleague2-pg).
  local stray
  stray="$(docker ps -aq --filter 'name=lleague' || true)"
  if [[ -n "$stray" ]]; then
    info "Removing stray lleague* containers..."
    docker rm -f $stray >/dev/null
    ok "Removed stray containers."
  fi
  ok "Docker cleaned. Start a fresh stack with: ./lleague.sh up --build"
}

cmd_restart() {  # ./lleague.sh restart [svc...]
  need docker; cd "$ROOT"
  compose restart "$@"
  ok "Restarted${*:+: $*}."
}

cmd_logs() {     # ./lleague.sh logs [svc...]
  need docker; cd "$ROOT"
  compose logs -f --tail=100 "$@"
}

cmd_ps() {       # ./lleague.sh ps / status
  need docker; cd "$ROOT"
  compose ps
}

# =========================================================================
# Dev / hot-reload mode
# =========================================================================
cmd_db() {       # ./lleague.sh db [up|down|psql]
  need docker; cd "$ROOT"
  case "${1:-up}" in
    up|"")  info "Starting Postgres container only..."; compose up -d db; wait_for_db ;;
    down)   info "Stopping Postgres container...";       compose stop db; ok "db stopped (data kept)." ;;
    psql)   info "Opening psql (Ctrl+D to exit)...";     docker exec -it "$DB_CONTAINER" psql -U postgres -d lleague ;;
    *)      die "Usage: ./lleague.sh db [up|down|psql]" ;;
  esac
}

cmd_api() {      # ./lleague.sh api — run the API locally with hot rebuild
  need dotnet
  info "Running API locally → $API_DEV_URL  (Ctrl+C to stop)"
  cd "$API_DIR"
  exec dotnet run
}

cmd_web() {      # ./lleague.sh web — run the web app locally with HMR
  need npm
  cd "$WEB_DIR"
  [[ -d node_modules ]] || { info "Installing web dependencies..."; npm install; }
  info "Running web dev server → $WEB_DEV_URL  (Ctrl+C to stop)"
  exec npm run dev
}

cmd_dev() {      # ./lleague.sh dev — db (docker) + api (dotnet) + web (vite), all hot
  need docker; need dotnet; need npm
  cd "$ROOT"

  info "Starting Postgres (Docker)..."
  compose up -d db
  wait_for_db

  [[ -d "$WEB_DIR/node_modules" ]] || { info "Installing web dependencies..."; (cd "$WEB_DIR" && npm install); }

  info "Launching API ($API_DEV_URL) and Web ($WEB_DEV_URL) with hot reload."
  printf '   %sLogin with your configured admin credentials  •  Ctrl+C stops both (db keeps running — ./lleague.sh db down)%s\n' "$C_DIM" "$C_OFF"

  # Kill the whole process group when this script exits / is interrupted.
  trap 'echo; info "Stopping dev servers..."; kill 0 2>/dev/null' EXIT INT TERM

  ( cd "$API_DIR" && dotnet run ) &
  ( cd "$WEB_DIR" && npm run dev ) &
  wait
}

# =========================================================================
# Database migrations (local dotnet-ef)
# =========================================================================
cmd_migrate() {  # ./lleague.sh migrate [Name]   — add migration (Name) OR apply pending
  need dotnet
  cd "$API_DIR"
  if ! dotnet ef --version >/dev/null 2>&1; then
    die "dotnet-ef not installed. Run: dotnet tool install --global dotnet-ef"
  fi
  if [[ -n "${1:-}" ]]; then
    info "Adding migration '$1'..."
    dotnet ef migrations add "$1"
    ok "Migration added. Apply it with: ./lleague.sh migrate   (or just run the api)"
  else
    info "Applying pending migrations to the dev database..."
    dotnet ef database update
    ok "Database up to date."
  fi
}

cmd_db_reset() { # ./lleague.sh dbreset — drop dev db and recreate from migrations
  need dotnet; cd "$API_DIR"
  warn "This drops the dev database (data lost), then recreates it from migrations."
  read -r -p "Type 'yes' to continue: " ans
  [[ "$ans" == "yes" ]] || { info "Aborted."; return; }
  dotnet ef database drop -f
  dotnet ef database update
  ok "Dev database recreated."
}

# =========================================================================
# Misc
# =========================================================================
cmd_open() {     # ./lleague.sh open — open the web app in a browser
  open_url "$WEB_DEV_URL"
}

cmd_clean() {    # ./lleague.sh clean — remove generated build artifacts (backend/artifacts, frontend/dist)
  info "Removing backend/artifacts (all .NET build + coverage output) and frontend/dist..."
  rm -rf "$ROOT/backend/artifacts" "$WEB_DIR/dist"
  # Drop any stale per-project bin/obj from before the artifacts-output layout, too.
  rm -rf "$API_DIR/bin" "$API_DIR/obj"
  ok "Cleaned. (node_modules left intact — use 'clean --all' to remove it too)"
  if [[ "${1:-}" == "--all" ]]; then
    info "Removing frontend/node_modules..."
    rm -rf "$WEB_DIR/node_modules"
    ok "node_modules removed."
  fi
}

cmd_help() {
  cat <<EOF
${C_BOLD}lleague.sh${C_OFF} — manage & run the LLeague2 stack (db + api + web)

${C_BOLD}Containerized stack${C_OFF} (production-like, all in Docker)
  ${C_GREEN}up${C_OFF} [--build]      start full stack detached       ($WEB_DOCKER_URL)
  ${C_GREEN}build${C_OFF} [svc]       (re)build image(s) only
  ${C_GREEN}rebuild${C_OFF} [svc]     rebuild + recreate running containers
  ${C_GREEN}restart${C_OFF} [svc]     restart service(s)
  ${C_GREEN}down${C_OFF}              stop stack, keep db data
  ${C_GREEN}reset${C_OFF}             stop stack and WIPE db volume
  ${C_GREEN}dockerclean${C_OFF} [--volumes]  remove project + stray lleague* containers (fresh 'up')
  ${C_GREEN}logs${C_OFF} [svc]        follow logs (all or one service)
  ${C_GREEN}ps${C_OFF} / status       show container status

${C_BOLD}Dev / hot-reload${C_OFF} (edit code, see it live)
  ${C_GREEN}dev${C_OFF}               db (Docker) + api (dotnet) + web (vite), all hot
  ${C_GREEN}db${C_OFF} [up|down|psql] manage only the Postgres container / open psql
  ${C_GREEN}api${C_OFF}               run only the API locally  ($API_DEV_URL)
  ${C_GREEN}web${C_OFF}               run only the web app locally ($WEB_DEV_URL)

${C_BOLD}Database${C_OFF}
  ${C_GREEN}migrate${C_OFF} [Name]    add migration <Name>, or apply pending if no name
  ${C_GREEN}dbreset${C_OFF}           drop & recreate the dev database from migrations

${C_BOLD}Misc${C_OFF}
  ${C_GREEN}open${C_OFF}              open the web app in your browser
  ${C_GREEN}clean${C_OFF} [--all]     remove bin/obj/dist (and node_modules with --all)
  ${C_GREEN}help${C_OFF}              this message

services: ${C_DIM}db, api, web${C_OFF}    Examples: ${C_DIM}./lleague.sh up --build  •  ./lleague.sh logs api  •  ./lleague.sh dev${C_OFF}
EOF
}

# --- Dispatch ------------------------------------------------------------
cmd="${1:-help}"; shift || true
case "$cmd" in
  up)             cmd_up "$@" ;;
  build)          cmd_build "$@" ;;
  rebuild)        cmd_rebuild "$@" ;;
  restart)        cmd_restart "$@" ;;
  down|stop)      cmd_down "$@" ;;
  reset)          cmd_reset "$@" ;;
  dockerclean)    cmd_dockerclean "$@" ;;
  logs)           cmd_logs "$@" ;;
  ps|status)      cmd_ps "$@" ;;
  dev)            cmd_dev "$@" ;;
  db)             cmd_db "$@" ;;
  api)            cmd_api "$@" ;;
  web)            cmd_web "$@" ;;
  migrate)        cmd_migrate "$@" ;;
  dbreset)        cmd_db_reset "$@" ;;
  open)           cmd_open "$@" ;;
  clean)          cmd_clean "$@" ;;
  help|-h|--help) cmd_help ;;
  *)              warn "Unknown command: $cmd"; echo; cmd_help; exit 1 ;;
esac
