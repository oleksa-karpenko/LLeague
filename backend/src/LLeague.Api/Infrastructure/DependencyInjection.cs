using System.Text;
using LLeague.Api.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LLeague.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing connection string 'Postgres'.");
        var jwtSettings = new JwtSettings
        {
            Secret = config["JWT_SECRET"] ?? throw new InvalidOperationException("Missing JWT_SECRET."),
            Issuer = "LLeague",
            ExpiryHours = 12
        };

        // ---- Persistence (EF Core is the unit of work; exposed via IAppDbContext) ----
        _ = services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        _ = services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // ---- Identity / security ----
        _ = services.AddSingleton(jwtSettings);
        _ = services.AddSingleton<ITokenService, JwtTokenService>();
        _ = services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        _ = services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true
                };
            });
        _ = services.AddAuthorization();

        return services;
    }
}
