using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using FluentValidation;
using MediatR;

namespace BackendTemplate.Application.Features.Files.Commands.UploadFile;

public record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string? Folder = null) : IRequest<Result<FileUploadResult>>;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg",
        ".pdf", ".doc", ".docx", ".txt", ".csv", ".xlsx"
    };

    public UploadFileCommandValidator()
    {
        RuleFor(v => v.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(HaveAllowedExtension).WithMessage("File type extension is not permitted for upload.");

        RuleFor(v => v.ContentType)
            .NotEmpty().WithMessage("Content type is required.");

        RuleFor(v => v.FileSizeBytes)
            .GreaterThan(0).WithMessage("Uploaded file cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize).WithMessage($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)} MB.");
    }

    private static bool HaveAllowedExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrEmpty(extension) && AllowedExtensions.Contains(extension);
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<FileUploadResult>>
{
    private readonly IFileStorageService _fileStorageService;

    public UploadFileCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<FileUploadResult>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        return await _fileStorageService.UploadFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            request.Folder,
            cancellationToken);
    }
}
