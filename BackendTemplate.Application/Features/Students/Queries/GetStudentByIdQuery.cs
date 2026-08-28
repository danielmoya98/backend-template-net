using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Features.Students.DTOs;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Features.Students.Queries;

public record GetStudentByIdQuery(Guid Id) : IRequest<Result<StudentDto>>;

public class GetStudentByIdQueryValidator : AbstractValidator<GetStudentByIdQuery>
{
    public GetStudentByIdQueryValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Student Id is required.");
    }
}

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new StudentDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email,
                s.PhoneNumber,
                s.DateOfBirth,
                s.IsActive,
                s.CreatedAt,
                s.CreatedBy))
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            return Result<StudentDto>.Failure(Error.NotFound("Students.NotFound", $"Student with Id '{request.Id}' was not found."));
        }

        return Result<StudentDto>.Success(student);
    }
}
