using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DomainError = BackendTemplate.Domain.Common.Error;

namespace BackendTemplate.Infrastructure.Services.FileStorage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary? _cloudinary;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(IConfiguration configuration, ILogger<CloudinaryFileStorageService> logger)
    {
        _logger = logger;
        var cloudName = configuration["FileStorage:Cloudinary:CloudName"];
        var apiKey = configuration["FileStorage:Cloudinary:ApiKey"];
        var apiSecret = configuration["FileStorage:Cloudinary:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary == null)
        {
            return Result<FileUploadResult>.Failure(DomainError.Failure(
                "FileStorage.CloudinaryNotConfigured",
                "Cloudinary credentials are not configured in FileStorage:Cloudinary settings."));
        }

        try
        {
            var isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            var uniqueFileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            UploadResult uploadResult;

            if (isImage)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = folder ?? "uploads",
                    PublicId = $"{uniqueFileNameWithoutExt}_{Guid.NewGuid():N}",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    Folder = folder ?? "uploads",
                    PublicId = $"{uniqueFileNameWithoutExt}_{Guid.NewGuid():N}",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
            }

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                return Result<FileUploadResult>.Failure(DomainError.Failure("FileStorage.CloudinaryUploadFailed", uploadResult.Error.Message));
            }

            var result = new FileUploadResult(
                FileUrl: uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
                PublicId: uploadResult.PublicId,
                FileName: fileName,
                FileSizeBytes: uploadResult.Bytes,
                ContentType: contentType);

            return Result<FileUploadResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception uploading file to Cloudinary: {FileName}", fileName);
            return Result<FileUploadResult>.Failure(DomainError.Failure("FileStorage.CloudinaryException", "Failed to upload file to Cloudinary."));
        }
    }

    public async Task<Result> DeleteFileAsync(string fileUrlOrPublicId, CancellationToken cancellationToken = default)
    {
        if (_cloudinary == null)
        {
            return Result.Failure(DomainError.Failure(
                "FileStorage.CloudinaryNotConfigured",
                "Cloudinary credentials are not configured."));
        }

        try
        {
            var deletionParams = new DeletionParams(fileUrlOrPublicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result == "ok" || deletionResult.Result == "not found")
            {
                return Result.Success();
            }

            return Result.Failure(DomainError.Failure("FileStorage.CloudinaryDeleteFailed", deletionResult.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception deleting file from Cloudinary: {PublicId}", fileUrlOrPublicId);
            return Result.Failure(DomainError.Failure("FileStorage.CloudinaryDeleteException", "Failed to delete file from Cloudinary."));
        }
    }
}
