using LLeague.Api.Application.Services;
using LLeague.Api.Domain;

namespace LLeague.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddSingleton<ScoringService>();   // stateless domain service

        _ = services.AddScoped<AuthService>();
        _ = services.AddScoped<SeasonService>();
        _ = services.AddScoped<EventService>();
        _ = services.AddScoped<TeamService>();
        _ = services.AddScoped<DivisionService>();
        _ = services.AddScoped<MatchService>();
        _ = services.AddScoped<ScoresheetService>();

        return services;
    }
}
