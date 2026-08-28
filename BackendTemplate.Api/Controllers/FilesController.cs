using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Files.Commands.DeleteFile;
using BackendTemplate.Application.Features.Files.Commands.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendTemplate.Api.Controllers;

[Authorize]
public class FilesController : ApiControllerBase
{
    /// <summary>
    /// Uploads a file (image, PDF, document) to the configured storage provider (Local, Cloudinary, or Supabase Storage).
    /// </summary>
    /// <param name="file">The multipart form file to upload.</param>
    /// <param name="folder">Optional target folder name (e.g. avatars, invoices).</param>
    /// <returns>Uploaded file metadata including URL and Public ID.</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<FileUploadResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Upload(IFormFile? file, [FromQuery] string? folder = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse
            {
                Success = false,
                Message = "No file was provided for upload.",
                Errors = new[] { "Please select a non-empty file to upload." }
            });
        }

        using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            FileStream: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length,
            Folder: folder);

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Deletes a file by its URL or Public ID from the configured storage provider.
    /// </summary>
    /// <param name="fileUrlOrPublicId">The URL or Public ID of the file to delete.</param>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromQuery] string fileUrlOrPublicId)
    {
        var command = new DeleteFileCommand(fileUrlOrPublicId);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
