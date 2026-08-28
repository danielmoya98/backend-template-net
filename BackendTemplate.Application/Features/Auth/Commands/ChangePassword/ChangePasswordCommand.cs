using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest<Result>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(v => v.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        return await _identityService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}
