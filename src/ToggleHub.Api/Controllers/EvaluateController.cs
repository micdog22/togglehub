
using Microsoft.AspNetCore.Mvc;
using ToggleHub.Core;

namespace ToggleHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluateController : ControllerBase
{
    private readonly IFlagStore _store;
    public EvaluateController(IFlagStore store) => _store = store;

    [HttpPost]
    public ActionResult<EvaluateResponse> Evaluate([FromBody] EvaluateRequest req)
    {
        var flag = _store.Get(req.FlagKey);
        var res = Engine.Evaluate(flag, req);
        return Ok(res);
    }
}
