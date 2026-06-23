using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
public class MatchesController(MatchService matches) : ControllerBase
{
    [HttpGet("divisions/{divisionId:guid}/matches")]
    public async Task<ActionResult<IEnumerable<MatchResponse>>> List(Guid divisionId)
    {
        return Ok(await matches.ListAsync(divisionId));
    }

    [HttpPost("divisions/{divisionId:guid}/matches")]
    public async Task<ActionResult<MatchResponse>> Create(Guid divisionId, MatchRequest req)
    {
        MatchResponse match = await matches.CreateAsync(divisionId, req);
        return CreatedAtAction(nameof(List), new { divisionId }, match);
    }

    [HttpPost("matches/{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        await matches.StartAsync(id);
        return NoContent();
    }

    [HttpPost("matches/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        await matches.CompleteAsync(id);
        return NoContent();
    }

    [HttpPost("matches/{id:guid}/abort")]
    public async Task<IActionResult> Abort(Guid id)
    {
        await matches.AbortAsync(id);
        return NoContent();
    }
}
