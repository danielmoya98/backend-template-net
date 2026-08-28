using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<Result>;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IIdentityService _identityService;

    public RevokeTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RevokeTokenAsync(
            request.RefreshToken,
            request.IpAddress,
            cancellationToken);
    }
}
