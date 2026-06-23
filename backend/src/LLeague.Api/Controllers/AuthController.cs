using System.Security.Claims;
using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        return Ok(await auth.LoginAsync(req));
    }

    // A protected probe so you can SEE the token working end-to-end.
    [HttpGet("me")]
    [Authorize]
    public ActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}
