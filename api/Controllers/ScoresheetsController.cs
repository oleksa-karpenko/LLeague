using LLeague.Api.Application;
using LLeague.Api.Application.Services;
using LLeague.Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLeague.Api.Controllers;

[ApiController]
[Authorize]
public class ScoresheetsController(ScoresheetService scoresheets) : ControllerBase
{
    // ---------- Mission catalog (the form definition for the UI) ----------

    [HttpGet("missions/catalog")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<MissionDef>> Catalog()
    {
        return Ok(MissionCatalog.Missions);
    }

    // ---------- Read a participant's scoresheet ----------

    [HttpGet("scoresheets/{participantId:guid}")]
    public async Task<ActionResult<ScoresheetResponse>> Get(Guid participantId)
    {
        return Ok(await scoresheets.GetAsync(participantId));
    }

    // ---------- Create or update (upsert) the values; server scores it ----------

    [HttpPut("scoresheets/{participantId:guid}")]
    public async Task<ActionResult<ScoresheetResponse>> Upsert(Guid participantId, ScoresheetUpdateRequest req)
    {
        return Ok(await scoresheets.UpsertAsync(participantId, req));
    }

    // ---------- Submit (finalize) ----------

    [HttpPost("scoresheets/{participantId:guid}/submit")]
    public async Task<ActionResult<ScoresheetResponse>> Submit(Guid participantId)
    {
        return Ok(await scoresheets.SubmitAsync(participantId));
    }
}
