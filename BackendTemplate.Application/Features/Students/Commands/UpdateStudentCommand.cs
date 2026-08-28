using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Features.Students.Commands;

public record UpdateStudentCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    bool IsActive) : IRequest<Result>;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Student Id is required.");

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
            .MustAsync(BeUniqueEmailExcludingSelf).WithMessage("Another student already uses this email address.");

        RuleFor(v => v.PhoneNumber)
            .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.");
    }

    private async Task<bool> BeUniqueEmailExcludingSelf(UpdateStudentCommand command, string email, CancellationToken cancellationToken)
    {
        return !await _context.Students
            .AnyAsync(s => s.Id != command.Id && s.Email.ToLower() == email.ToLower(), cancellationToken);
    }
}

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            return Result.Failure(Error.NotFound("Students.NotFound", $"Student with Id '{request.Id}' was not found."));
        }

        student.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
