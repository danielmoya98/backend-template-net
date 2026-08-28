using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Auth.Commands.ChangePassword;
using BackendTemplate.Application.Features.Auth.Commands.Login;
using BackendTemplate.Application.Features.Auth.Commands.RefreshToken;
using BackendTemplate.Application.Features.Auth.Commands.Register;
using BackendTemplate.Application.Features.Auth.Commands.RevokeToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new LoginCommand(request.Email, request.Password, ipAddress);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName, request.Role);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken, ipAddress);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPost("revoke-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var command = new RevokeTokenCommand(request.RefreshToken, ipAddress);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
}
