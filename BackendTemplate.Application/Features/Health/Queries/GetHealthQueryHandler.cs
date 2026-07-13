using MediatR;

namespace BackendTemplate.Application.Features.Health.Queries;

public class GetHealthQueryHandler : IRequestHandler<GetHealthQuery, string>
{
    public Task<string> Handle(GetHealthQuery request, CancellationToken cancellationToken)
    {
        // Aquí iría tu lógica de negocio, consultas a la BD usando repositorios, etc.
        return Task.FromResult("Template API is up and running securely! 🚀");
    }
}
