using MediatR;

namespace BackendTemplate.Application.Features.Health.Queries;

// IRequest<string> indica que este query devolverá un string
public record GetHealthQuery : IRequest<string>;
