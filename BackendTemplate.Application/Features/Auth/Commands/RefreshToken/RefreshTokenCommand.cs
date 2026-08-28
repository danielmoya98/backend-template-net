using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string? IpAddress = null) : IRequest<Result<AuthResponse>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.AccessToken)
            .NotEmpty().WithMessage("AccessToken is required.");

        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(
            request.AccessToken,
            request.RefreshToken,
            request.IpAddress,
            cancellationToken);
    }
}
