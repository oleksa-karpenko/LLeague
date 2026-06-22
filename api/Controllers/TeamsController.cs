using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
[Route("teams")]
public class TeamsController(TeamService teams) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<TeamResponse>> List()
    {
        return teams.ListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamResponse>> Get(Guid id)
    {
        return Ok(await teams.GetAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> Create(TeamRequest req)
    {
        TeamResponse t = await teams.CreateAsync(req);
        return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TeamResponse>> Update(Guid id, TeamRequest req)
    {
        return Ok(await teams.UpdateAsync(id, req));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await teams.DeleteAsync(id);
        return NoContent();
    }
}
