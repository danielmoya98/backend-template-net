using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    // Transforma el Result del Dominio en un HTTP Response estandarizado
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<T> { Data = result.Value, Message = "Request successful." });
        }

        return BadRequest(new ApiErrorResponse { Message = result.ErrorMessage ?? "An error occurred." });
    }
}
