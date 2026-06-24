---
mode: agent
description: Add/modify an EF Core entity and generate a migration
---

Add or change a persisted entity and produce the matching EF Core migration.

1. Edit/add the entity in `Domain/` (plain class, no EF attributes unless necessary; nullable enabled).
2. If it's a new aggregate, add a `DbSet<T>` to **both** `Application/Abstractions/IAppDbContext.cs`
   **and** `Infrastructure/AppDbContext.cs`, plus any `OnModelCreating` configuration.
3. Generate the migration:
   ```bash
   cd backend/src/LLeague.Api
   dotnet ef migrations add <DescriptiveName> -o Infrastructure/Migrations
   ```
4. Review the generated migration for correctness (column types, indexes, FK cascade behavior).
5. If seed data is needed, update `Infrastructure/DbSeeder.cs`.
6. Build + format: `cd backend && dotnet format LLeague.slnx && dotnet build LLeague.slnx`.

Do not edit already-applied migrations — always add a new one. Tell me what the migration changes.
