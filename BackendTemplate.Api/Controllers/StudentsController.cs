using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Students.Commands;
using BackendTemplate.Application.Features.Students.DTOs;
using BackendTemplate.Application.Features.Students.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

[Authorize]
public class StudentsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<StudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWithPagination([FromQuery] GetStudentsWithPaginationQuery query)
    {
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetStudentByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateStudentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new ApiErrorResponse
            {
                Success = false,
                Message = "Route Id parameter does not match payload Id.",
                Errors = new[] { "Id mismatch between URL route and request body." }
            });
        }

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteStudentCommand(id));
        return HandleResult(result);
    }
}
