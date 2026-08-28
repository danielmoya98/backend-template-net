using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress = null) : IRequest<Result<AuthResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(
            new LoginRequest(request.Email, request.Password),
            request.IpAddress,
            cancellationToken);
    }
}
