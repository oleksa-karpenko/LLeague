# Code coverage

How to measure, view, and browse test coverage for the API.

## TL;DR

```bash
./coverage.sh
```

Runs the test suite with coverage, builds the HTML report (migrations excluded),
prints a text summary, and opens the report in your browser. Use `./coverage.sh --no-open`
to skip launching the browser (e.g. on CI).

> Docker must be running — the integration tests spin up a throwaway Postgres
> container via Testcontainers. The unit tests (`ScoringServiceTests`) don't need it.

## How it works

Coverage is collected by **coverlet** (the `coverlet.collector` package on the test
project) during `dotnet test`, producing a Cobertura XML file. **reportgenerator**
(a global dotnet tool) turns that XML into a browsable HTML site.

The one-time setup is already applied:

- `tests/LLeague.Api.Tests` references `coverlet.collector`.
- `dotnet-reportgenerator-globaltool` is installed (`coverage.sh` installs it if missing).

## Manual commands

If you'd rather run the steps yourself instead of `./coverage.sh`:

```bash
# reportgenerator lives in the dotnet tools dir — put it on PATH (add to ~/.zshrc to persist)
export PATH="$PATH:$HOME/.dotnet/tools"

cd <repo-root>

# 1. Run tests + collect coverage into artifacts/ (clear old results first)
rm -rf artifacts/test-results artifacts/coverage
dotnet test LLeague.slnx --collect:"XPlat Code Coverage" --results-directory artifacts/test-results

# 2. Build the HTML report (migrations + generated code excluded for an honest number)
reportgenerator \
  -reports:"artifacts/test-results/**/coverage.cobertura.xml" \
  -targetdir:artifacts/coverage \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"-LLeague.Api.Migrations.*;-*Generated*;-System.*;-Microsoft.*"

# 3. View it
open artifacts/coverage/index.html      # or: cat artifacts/coverage/Summary.txt
```

## Browsing the HTML report

Open `artifacts/coverage/index.html`, then:

- **Top table** — per-class line & branch %. Click a class name to drill into its source.
- **Class detail** — full source with **green = covered, red = uncovered** lines; hit
  counts and per-branch coverage in the left margin.
- **Risk Hotspots tab** — methods ranked by low coverage × high complexity, i.e. where
  adding tests pays off most.
- **Filter box** — type a class name (e.g. `MatchesController`) to jump to it.

You can also serve it instead of opening the file directly:

```bash
python3 -m http.server -d artifacts/coverage 8090   # then visit http://localhost:8090
```

## Reading the numbers

EF migrations and compiler/source-generated classes (OpenAPI, regex generators, async state
machines) are excluded via `-classfilters`, so the reported percentage reflects hand-written
code. The active filter is:

```
-classfilters:"-LLeague.Api.Migrations.*;-*Generated*;-System.*;-Microsoft.*"
```

Focus on **per-class** coverage of the logic-heavy controllers/services rather than the
single overall number. Drop the `-classfilters` flag if you ever want the raw,
unfiltered figures.

## Notes

- All output lands under `artifacts/` (coverage in `artifacts/coverage`, raw results in
  `artifacts/test-results`), alongside the .NET build output. The whole `artifacts/` folder
  is git-ignored and safe to delete — `./ll.sh clean` removes it.
