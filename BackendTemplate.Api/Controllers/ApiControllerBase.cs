using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Data = result.Value,
                Message = "Request processed successfully."
            });
        }

        return HandleError(result.Error);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<object?>
            {
                Success = true,
                Data = null,
                Message = "Request processed successfully."
            });
        }

        return HandleError(result.Error);
    }

    private IActionResult HandleError(Error error)
    {
        var errorResponse = new ApiErrorResponse
        {
            Success = false,
            Message = error.Description,
            Errors = new[] { $"{error.Code}: {error.Description}" }
        };

        return error.Type switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
