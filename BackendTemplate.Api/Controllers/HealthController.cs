using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Application.Features.Health.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

public class HealthController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await Mediator.Send(new GetHealthQuery());
        return HandleResult(result);
    }
}
