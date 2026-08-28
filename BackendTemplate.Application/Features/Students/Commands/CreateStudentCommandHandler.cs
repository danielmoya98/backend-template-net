using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.Entities;
using MediatR;

namespace BackendTemplate.Application.Features.Students.Commands;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DateOfBirth);

        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(student.Id);
    }
}
