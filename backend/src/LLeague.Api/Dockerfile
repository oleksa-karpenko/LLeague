# Build context is the repo's backend/ folder (see docker-compose.yml: context ./backend),
# so this build shares the single backend/nuget.config and Directory.Build.props.
# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Restore against just the manifests first for better layer caching.
COPY nuget.config Directory.Build.props ./
COPY src/LLeague.Api/LLeague.Api.csproj src/LLeague.Api/
RUN dotnet restore src/LLeague.Api/LLeague.Api.csproj
# Then bring in the rest of the source and publish.
COPY src/LLeague.Api/ src/LLeague.Api/
RUN dotnet publish src/LLeague.Api/LLeague.Api.csproj -c Release -o /app /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LLeague.Api.dll"]
