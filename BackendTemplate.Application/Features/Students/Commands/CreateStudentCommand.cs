using BackendTemplate.Domain.Common;
using MediatR;

namespace BackendTemplate.Application.Features.Students.Commands;

public record CreateStudentCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber = null,
    DateTime? DateOfBirth = null) : IRequest<Result<Guid>>;
