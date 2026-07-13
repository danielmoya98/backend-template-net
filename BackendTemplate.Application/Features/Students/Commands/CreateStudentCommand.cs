using MediatR;
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Application.Features.Students.Commands;

public record CreateStudentCommand(string FirstName, string LastName, string Email) : IRequest<Result<Guid>>;
