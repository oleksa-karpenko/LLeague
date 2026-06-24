using LLeague.Api.Domain;

namespace LLeague.Api.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(AdminUser user);
}
