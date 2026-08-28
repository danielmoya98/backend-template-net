using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Infrastructure.Services.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        var configPath = configuration["FileStorage:Local:StoragePath"] ?? "wwwroot/uploads";
        _basePath = Path.IsPathRooted(configPath)
            ? configPath
            : Path.Combine(Directory.GetCurrentDirectory(), configPath);

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetFolder = string.IsNullOrWhiteSpace(folder)
                ? _basePath
                : Path.Combine(_basePath, folder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(targetFolder, uniqueFileName);

            using (var destinationStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(destinationStream, cancellationToken);
            }

            var relativePath = string.IsNullOrWhiteSpace(folder)
                ? $"/uploads/{uniqueFileName}"
                : $"/uploads/{folder.Replace('\\', '/')}/{uniqueFileName}";

            var result = new FileUploadResult(
                FileUrl: relativePath,
                PublicId: uniqueFileName,
                FileName: fileName,
                FileSizeBytes: fileStream.Length,
                ContentType: contentType);

            return Result<FileUploadResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file locally: {FileName}", fileName);
            return Result<FileUploadResult>.Failure(Error.Failure("FileStorage.LocalUploadError", "Could not save file locally."));
        }
    }

    public Task<Result> DeleteFileAsync(string fileUrlOrPublicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrlOrPublicId);
            var files = Directory.GetFiles(_basePath, fileName, SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                File.Delete(files[0]);
                return Task.FromResult(Result.Success());
            }

            return Task.FromResult(Result.Failure(Error.NotFound("FileStorage.FileNotFound", "File to delete was not found.")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting local file: {File}", fileUrlOrPublicId);
            return Task.FromResult(Result.Failure(Error.Failure("FileStorage.LocalDeleteError", "Could not delete local file.")));
        }
    }
}
