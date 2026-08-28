using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = null) : IRequest<Result<string>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(
            new RegisterRequest(request.Email, request.Password, request.FirstName, request.LastName, request.Role),
            cancellationToken);
    }
}
