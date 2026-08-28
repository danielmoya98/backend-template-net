using System.Net.Http.Headers;
using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackendTemplate.Infrastructure.Services.FileStorage;

public class SupabaseFileStorageService : IFileStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string? _supabaseUrl;
    private readonly string? _apiKey;
    private readonly string _bucketName;
    private readonly ILogger<SupabaseFileStorageService> _logger;

    public SupabaseFileStorageService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SupabaseFileStorageService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _supabaseUrl = configuration["FileStorage:Supabase:Url"]?.TrimEnd('/');
        _apiKey = configuration["FileStorage:Supabase:ApiKey"];
        _bucketName = configuration["FileStorage:Supabase:BucketName"] ?? "uploads";
    }

    public async Task<Result<FileUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_apiKey))
        {
            return Result<FileUploadResult>.Failure(Error.Failure(
                "FileStorage.SupabaseNotConfigured",
                "Supabase Url or ApiKey is missing in FileStorage:Supabase settings."));
        }

        try
        {
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var objectPath = string.IsNullOrWhiteSpace(folder)
                ? uniqueFileName
                : $"{folder.Trim('/')}/{uniqueFileName}";

            var requestUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{objectPath}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Add("apikey", _apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Supabase upload failed ({StatusCode}): {Body}", response.StatusCode, errorBody);
                return Result<FileUploadResult>.Failure(Error.Failure("FileStorage.SupabaseUploadFailed", $"Supabase upload returned {response.StatusCode}."));
            }

            var publicUrl = $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{objectPath}";

            var result = new FileUploadResult(
                FileUrl: publicUrl,
                PublicId: objectPath,
                FileName: fileName,
                FileSizeBytes: fileStream.Length,
                ContentType: contentType);

            return Result<FileUploadResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception uploading file to Supabase: {FileName}", fileName);
            return Result<FileUploadResult>.Failure(Error.Failure("FileStorage.SupabaseException", "Failed to upload file to Supabase Storage."));
        }
    }

    public async Task<Result> DeleteFileAsync(string fileUrlOrPublicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_apiKey))
        {
            return Result.Failure(Error.Failure(
                "FileStorage.SupabaseNotConfigured",
                "Supabase credentials are not configured."));
        }

        try
        {
            var objectPath = fileUrlOrPublicId.Contains("/storage/v1/object/public/")
                ? fileUrlOrPublicId.Substring(fileUrlOrPublicId.IndexOf(_bucketName + "/", StringComparison.Ordinal) + _bucketName.Length + 1)
                : fileUrlOrPublicId;

            var requestUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{objectPath}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            request.Headers.Add("apikey", _apiKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result.Success();
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Supabase delete failed ({StatusCode}): {Body}", response.StatusCode, errorBody);
            return Result.Failure(Error.Failure("FileStorage.SupabaseDeleteFailed", $"Supabase delete returned {response.StatusCode}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception deleting file from Supabase: {Path}", fileUrlOrPublicId);
            return Result.Failure(Error.Failure("FileStorage.SupabaseDeleteException", "Failed to delete file from Supabase Storage."));
        }
    }
}
