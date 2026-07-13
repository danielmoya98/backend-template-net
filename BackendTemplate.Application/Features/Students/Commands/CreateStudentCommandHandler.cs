using MediatR;
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Application.Features.Students.Commands;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Como pasó por el ValidationBehavior, aquí tienes GARANTÍA ABSOLUTA
        // de que FirstName, LastName y Email tienen datos válidos.
        
        // Aquí instanciarías la entidad y usarías el DbContext/Repository para guardar...
        var simulatedId = Guid.NewGuid();
        
        return await Task.FromResult(Result<Guid>.Success(simulatedId));
    }
}
