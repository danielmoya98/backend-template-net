using BackendTemplate.Domain.Common;
using MediatR;

namespace BackendTemplate.Application.Features.Health.Queries;

public class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<string>.Success("Healthy"));
    }
}
