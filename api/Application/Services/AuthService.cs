using LLeague.Api.Application.Abstractions;
using LLeague.Api.Domain;
using LLeague.Api.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LLeague.Api.Application.Services;

public class AuthService(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens)
{
    public async Task<LoginResponse> LoginAsync(LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            throw new ValidationException("Username and password are required");
        }

        AdminUser? user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !hasher.Verify(req.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        return new LoginResponse(tokens.CreateToken(user));
    }
}
