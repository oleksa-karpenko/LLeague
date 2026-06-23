#!/usr/bin/env bash
#
# Run the test suite with code coverage, build an HTML report, and present results.
# See coverage.md for details.
#
# Usage:
#   ./coverage.sh            run tests + coverage, print summary, open the HTML report
#   ./coverage.sh --no-open  same, but don't launch a browser (e.g. CI)
#
# Requires: .NET SDK, Docker running (for the integration tests' Postgres container).

set -euo pipefail

# Repo root = directory containing this script.
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

# reportgenerator is installed as a global dotnet tool; make sure it's on PATH.
export PATH="$PATH:$HOME/.dotnet/tools"

SOLUTION="LLeague.slnx"
TEST_RESULTS="artifacts/test-results"
REPORT_DIR="artifacts/coverage"

# Install the report generator the first time if it's missing.
if ! command -v reportgenerator >/dev/null 2>&1; then
  echo "==> Installing dotnet-reportgenerator-globaltool..."
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

echo "==> Running tests with coverage (Docker must be running for integration tests)..."
rm -rf "$TEST_RESULTS" "$REPORT_DIR"
dotnet test "$SOLUTION" --collect:"XPlat Code Coverage" --results-directory "$TEST_RESULTS"

echo "==> Generating HTML report (migrations + generated code excluded)..."
reportgenerator \
  -reports:"$TEST_RESULTS/**/coverage.cobertura.xml" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"-LLeague.Api.Migrations.*;-*Generated*;-System.*;-Microsoft.*"

echo
echo "==================== COVERAGE SUMMARY ===================="
cat "$REPORT_DIR/Summary.txt"
echo "========================================================="
echo "Full report: $ROOT/$REPORT_DIR/index.html"

if [[ "${1:-}" == "--no-open" ]]; then
  exit 0
fi

echo "==> Opening report..."
if command -v open >/dev/null 2>&1; then
  open "$REPORT_DIR/index.html"
elif command -v xdg-open >/dev/null 2>&1; then
  xdg-open "$REPORT_DIR/index.html"
else
  echo "(No 'open'/'xdg-open' found — open $REPORT_DIR/index.html manually.)"
fi
