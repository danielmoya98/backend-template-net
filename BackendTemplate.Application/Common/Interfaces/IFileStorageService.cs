using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;

namespace BackendTemplate.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        string fileUrlOrPublicId,
        CancellationToken cancellationToken = default);
}
