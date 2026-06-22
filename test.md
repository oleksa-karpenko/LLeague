# Run the whole suite (unit + integration)
  dotnet test LLeague.slnx

  Other useful variants:

  # Just the test project (same thing, explicit path)
  dotnet test tests/LLeague.Api.Tests

  # Only the fast pure-unit tests (no Docker/container needed)
  dotnet test LLeague.slnx --filter "FullyQualifiedName~ScoringServiceTests"

  # A single class or test
  dotnet test LLeague.slnx --filter "FullyQualifiedName~MatchTests"
  dotnet test LLeague.slnx --filter "FullyQualifiedName~Match_lifecycle_start_complete_abort"

  # Verbose output when something fails
  dotnet test LLeague.slnx --logger "console;verbosity=detailed"

  Notes:
  - First run is slower if the postgres:17 image isn't cached (it is on this machine, via docker-compose).
  - The integration tests use their own ephemeral container, not your dev DB (lleague-db on 5544) — so running them won't touch your dev data.
  - In Rider/VS Code you can also just hit the run/? gutter icons on any test now that LLeague.slnx ties the projects together.
