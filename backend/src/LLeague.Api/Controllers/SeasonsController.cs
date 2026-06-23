using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
[Route("seasons")]
public class SeasonsController(SeasonService seasons) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<SeasonResponse>> List()
    {
        return seasons.ListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SeasonResponse>> Get(Guid id)
    {
        return Ok(await seasons.GetAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<SeasonResponse>> Create(SeasonRequest req)
    {
        SeasonResponse season = await seasons.CreateAsync(req);
        return CreatedAtAction(nameof(Get), new { id = season.Id }, season);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SeasonResponse>> Update(Guid id, SeasonRequest req)
    {
        return Ok(await seasons.UpdateAsync(id, req));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await seasons.DeleteAsync(id);
        return NoContent();
    }
}
