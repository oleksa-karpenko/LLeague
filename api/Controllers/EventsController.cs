using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
[Route("events")]
public class EventsController(EventService events) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<EventResponse>> List([FromQuery] Guid? seasonId)
    {
        return events.ListAsync(seasonId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id)
    {
        return Ok(await events.GetAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(EventRequest req)
    {
        EventResponse e = await events.CreateAsync(req);
        return CreatedAtAction(nameof(Get), new { id = e.Id }, e);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, EventRequest req)
    {
        return Ok(await events.UpdateAsync(id, req));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await events.DeleteAsync(id);
        return NoContent();
    }
}
