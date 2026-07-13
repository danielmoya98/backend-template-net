using BackendTemplate.Application.Features.Health.Queries;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

public class HealthController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Enviamos el Query. MediatR sabrá que debe ejecutar GetHealthQueryHandler.
        var result = await Mediator.Send(new GetHealthQuery());
        
        return Ok(new { status = result });
    }
}
