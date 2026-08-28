using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Files.Commands.DeleteFile;

public record DeleteFileCommand(string FileUrlOrPublicId) : IRequest<Result>;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(v => v.FileUrlOrPublicId)
            .NotEmpty().WithMessage("File URL or Public ID is required.");
    }
}

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        return await _fileStorageService.DeleteFileAsync(request.FileUrlOrPublicId, cancellationToken);
    }
}
