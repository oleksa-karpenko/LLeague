#!/usr/bin/env bash
#
# Run the test suite with code coverage, build a report, enforce a line-coverage gate,
# and present results. The SAME script is used locally and in CI — it auto-detects
# GitHub Actions (or pass --ci) and adjusts behavior. See coverage.md for details.
#
# Usage:
#   ./coverage.sh            local: run + report, enforce the gate, open the HTML report
#   ./coverage.sh --no-open  local, but don't launch a browser
#   ./coverage.sh --ci       CI:    no browser, emit SVG badges + a GitHub step summary,
#                                   fail the run if below the gate (auto-on under Actions)
#
# Env:
#   COVERAGE_MIN   minimum line coverage % required to pass     (default: 60)
#   CONFIGURATION  build configuration for the test run         (default: Release)
#
# Requires: .NET SDK, Docker running (for the integration tests' Postgres container).

set -euo pipefail

# Repo backend root = directory containing this script.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

# reportgenerator is installed as a global dotnet tool; make sure it's on PATH.
export PATH="$PATH:$HOME/.dotnet/tools"

SOLUTION="LLeague.slnx"
CONFIGURATION="${CONFIGURATION:-Release}"
TEST_RESULTS="artifacts/test-results"
REPORT_DIR="artifacts/coverage"
COVERAGE_MIN="${COVERAGE_MIN:-60}"
CLASS_FILTERS="-LLeague.Api.Migrations.*;-*Generated*;-System.*;-Microsoft.*"

# --- Mode detection ------------------------------------------------------
# CI mode if --ci is passed, or we're running under GitHub Actions / a generic CI.
CI_MODE=false
OPEN=true
for arg in "$@"; do
  case "$arg" in
    --ci)      CI_MODE=true ;;
    --no-open) OPEN=false ;;
  esac
done
if [[ "${GITHUB_ACTIONS:-}" == "true" || "${CI:-}" == "true" ]]; then
  CI_MODE=true
fi
# Never pop a browser in CI.
$CI_MODE && OPEN=false

# In CI also emit the GitHub-flavored markdown summary and the SVG coverage badges.
if $CI_MODE; then
  REPORT_TYPES="Html;TextSummary;MarkdownSummaryGithub;Badges"
else
  REPORT_TYPES="Html;TextSummary"
fi

# --- Tooling -------------------------------------------------------------
# Install the report generator the first time if it's missing (covers CI's clean runner).
if ! command -v reportgenerator >/dev/null 2>&1; then
  echo "==> Installing dotnet-reportgenerator-globaltool..."
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# --- Run tests + coverage ------------------------------------------------
echo "==> Running tests with coverage ($CONFIGURATION; Docker required for integration tests)..."
rm -rf "$TEST_RESULTS" "$REPORT_DIR"
dotnet test "$SOLUTION" --configuration "$CONFIGURATION" \
  --collect:"XPlat Code Coverage" --results-directory "$TEST_RESULTS"

# --- Report --------------------------------------------------------------
echo "==> Generating coverage report (migrations + generated code excluded)..."
reportgenerator \
  -reports:"$TEST_RESULTS/**/coverage.cobertura.xml" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:"$REPORT_TYPES" \
  -classfilters:"$CLASS_FILTERS"

echo
echo "==================== COVERAGE SUMMARY ===================="
cat "$REPORT_DIR/Summary.txt"
echo "========================================================="
echo "Full report: $ROOT/$REPORT_DIR/index.html"

# Pull the line-coverage % out of the text summary (portable: no grep -P / BSD-safe).
LINE_COV=$(grep -oE 'Line coverage:[[:space:]]*[0-9.]+' "$REPORT_DIR/Summary.txt" \
            | grep -oE '[0-9.]+' | head -1)
echo "Line coverage: ${LINE_COV}%   (gate: ${COVERAGE_MIN}%)"

# In CI, surface the markdown summary on the workflow run page.
if $CI_MODE && [[ -n "${GITHUB_STEP_SUMMARY:-}" && -f "$REPORT_DIR/SummaryGithub.md" ]]; then
  cat "$REPORT_DIR/SummaryGithub.md" >> "$GITHUB_STEP_SUMMARY"
fi

# Open the report locally before the gate so a failure still leaves it on screen.
if $OPEN; then
  echo "==> Opening report..."
  if command -v open >/dev/null 2>&1; then open "$REPORT_DIR/index.html"
  elif command -v xdg-open >/dev/null 2>&1; then xdg-open "$REPORT_DIR/index.html"
  else echo "(No 'open'/'xdg-open' found — open $REPORT_DIR/index.html manually.)"; fi
fi

# --- Gate ----------------------------------------------------------------
if awk -v c="$LINE_COV" -v min="$COVERAGE_MIN" 'BEGIN { exit (c + 0 < min + 0) }'; then
  echo "✓ Coverage gate passed (${LINE_COV}% ≥ ${COVERAGE_MIN}%)."
else
  MSG="Line coverage ${LINE_COV}% is below the ${COVERAGE_MIN}% gate."
  $CI_MODE && echo "::error::${MSG}"   # GitHub annotation on the run
  echo "✗ ${MSG}" >&2
  exit 1
fi
