
using Microsoft.AspNetCore.Mvc;
using ToggleHub.Api.Auth;
using ToggleHub.Core;

namespace ToggleHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireApiKey]
public class FlagsController : ControllerBase
{
    private readonly IFlagStore _store;
    public FlagsController(IFlagStore store) => _store = store;

    [HttpGet]
    public ActionResult<IEnumerable<Flag>> List() => Ok(_store.List());

    [HttpGet("{key}")]
    public ActionResult<Flag> Get(string key)
    {
        var f = _store.Get(key);
        return f is null ? NotFound() : Ok(f);
    }

    [HttpPost]
    public ActionResult<Flag> Create([FromBody] UpsertFlagRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Key)) return BadRequest("key obrigatório");
        var flag = new Flag
        {
            Key = req.Key,
            Description = req.Description,
            Enabled = req.Enabled,
            RolloutPercent = Math.Clamp(req.RolloutPercent, 0, 100),
            IncludeUserIds = req.IncludeUserIds ?? new(),
            ExcludeUserIds = req.ExcludeUserIds ?? new(),
            MatchAnyAttributes = req.MatchAnyAttributes ?? new()
        };
        _store.Upsert(flag);
        return CreatedAtAction(nameof(Get), new { key = flag.Key }, flag);
    }

    [HttpPut("{key}")]
    public ActionResult<Flag> Update(string key, [FromBody] UpsertFlagRequest req)
    {
        var existing = _store.Get(key);
        if (existing is null) return NotFound();

        var updated = existing with
        {
            Description = req.Description ?? existing.Description,
            Enabled = req.Enabled,
            RolloutPercent = Math.Clamp(req.RolloutPercent, 0, 100),
            IncludeUserIds = req.IncludeUserIds ?? existing.IncludeUserIds,
            ExcludeUserIds = req.ExcludeUserIds ?? existing.ExcludeUserIds,
            MatchAnyAttributes = req.MatchAnyAttributes ?? existing.MatchAnyAttributes
        };
        _store.Upsert(updated);
        return Ok(updated);
    }

    [HttpDelete("{key}")]
    public ActionResult Delete(string key)
    {
        return _store.Delete(key) ? NoContent() : NotFound();
    }

    [HttpPost("{key}/toggle")]
    public ActionResult Toggle(string key, [FromQuery] bool enabled = true)
    {
        var f = _store.Get(key);
        if (f is null) return NotFound();
        _store.Toggle(key, enabled);
        return NoContent();
    }

    [HttpPost("import")]
    public ActionResult Import([FromBody] IEnumerable<Flag> flags)
    {
        _store.Import(flags);
        return NoContent();
    }

    [HttpGet("export")]
    public ActionResult<IEnumerable<Flag>> Export()
    {
        return Ok(_store.List());
    }
}
