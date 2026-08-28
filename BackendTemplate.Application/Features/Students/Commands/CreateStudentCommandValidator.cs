using BackendTemplate.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Features.Students.Commands;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateStudentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .MustAsync(BeUniqueEmail).WithMessage("A student with this email address already exists.");

        RuleFor(v => v.PhoneNumber)
            .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _context.Students
            .AnyAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}
