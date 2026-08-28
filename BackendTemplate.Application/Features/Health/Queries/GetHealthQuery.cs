using BackendTemplate.Domain.Common;
using MediatR;

namespace BackendTemplate.Application.Features.Health.Queries;

public record GetHealthQuery : IRequest<Result<string>>;
