using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using Microsoft.IdentityModel.Tokens;

namespace LLeague.Api.Infrastructure;

// Simple settings holder we register in DI.
public class JwtSettings
{
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "LLeague";
    public int ExpiryHours { get; set; } = 12;
}

public class JwtTokenService(JwtSettings settings) : ITokenService
{
    public string CreateToken(AdminUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("role", "admin")
        ];

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(settings.ExpiryHours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
