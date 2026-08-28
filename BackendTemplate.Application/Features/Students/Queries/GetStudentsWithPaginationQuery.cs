using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Students.DTOs;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BackendTemplate.Application.Features.Students.Queries;

public record GetStudentsWithPaginationQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<Result<PaginatedResult<StudentDto>>>;

public class GetStudentsWithPaginationQueryValidator : AbstractValidator<GetStudentsWithPaginationQuery>
{
    public GetStudentsWithPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.")
            .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100.");
    }
}

public class GetStudentsWithPaginationQueryHandler : IRequestHandler<GetStudentsWithPaginationQuery, Result<PaginatedResult<StudentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentsWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<StudentDto>>> Handle(GetStudentsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(search) ||
                s.LastName.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var projectedQuery = query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StudentDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.Email,
                s.PhoneNumber,
                s.DateOfBirth,
                s.IsActive,
                s.CreatedAt,
                s.CreatedBy));

        var result = await PaginatedResult<StudentDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PaginatedResult<StudentDto>>.Success(result);
    }
}
