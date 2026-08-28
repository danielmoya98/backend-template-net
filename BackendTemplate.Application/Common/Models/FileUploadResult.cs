namespace BackendTemplate.Application.Common.Models;

public record FileUploadResult(
    string FileUrl,
    string PublicId,
    string FileName,
    long FileSizeBytes,
    string ContentType);
