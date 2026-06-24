using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
[Route("divisions")]
public class DivisionsController(DivisionService divisions) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<DivisionResponse>> List([FromQuery] Guid? eventId)
    {
        return divisions.ListAsync(eventId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DivisionResponse>> Get(Guid id)
    {
        return Ok(await divisions.GetAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<DivisionResponse>> Create(DivisionRequest req)
    {
        DivisionResponse d = await divisions.CreateAsync(req);
        return CreatedAtAction(nameof(Get), new { id = d.Id }, d);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DivisionResponse>> Update(Guid id, DivisionRequest req)
    {
        return Ok(await divisions.UpdateAsync(id, req));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await divisions.DeleteAsync(id);
        return NoContent();
    }

    // ---------- Team enrollment ----------

    [HttpGet("{id:guid}/teams")]
    public async Task<ActionResult<IEnumerable<EnrolledTeamResponse>>> EnrolledTeams(Guid id)
    {
        return Ok(await divisions.EnrolledTeamsAsync(id));
    }

    [HttpPost("{id:guid}/teams")]
    public async Task<IActionResult> Enroll(Guid id, EnrollRequest req)
    {
        await divisions.EnrollAsync(id, req);
        return NoContent();
    }

    [HttpPost("{id:guid}/teams/{teamId:guid}/arrive")]
    public async Task<IActionResult> MarkArrival(Guid id, Guid teamId)
    {
        await divisions.MarkArrivalAsync(id, teamId);
        return NoContent();
    }

    // ---------- Tables ----------

    [HttpGet("{id:guid}/tables")]
    public async Task<ActionResult<IEnumerable<TableResponse>>> Tables(Guid id)
    {
        return Ok(await divisions.TablesAsync(id));
    }

    [HttpPost("{id:guid}/tables")]
    public async Task<ActionResult<TableResponse>> CreateTable(Guid id, TableRequest req)
    {
        return Ok(await divisions.CreateTableAsync(id, req));
    }

    // ---------- Standings ----------

    [HttpGet("{id:guid}/standings")]
    public async Task<ActionResult<IEnumerable<StandingRow>>> Standings(Guid id)
    {
        return Ok(await divisions.StandingsAsync(id));
    }
}
