using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Features.Students.Commands;

public record DeleteStudentCommand(Guid Id) : IRequest<Result>;

public class DeleteStudentCommandValidator : AbstractValidator<DeleteStudentCommand>
{
    public DeleteStudentCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Student Id is required.");
    }
}

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            return Result.Failure(Error.NotFound("Students.NotFound", $"Student with Id '{request.Id}' was not found."));
        }

        student.Delete();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
