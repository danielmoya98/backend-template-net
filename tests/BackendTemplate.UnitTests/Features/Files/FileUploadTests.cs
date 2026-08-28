using System.Text;
using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Files.Commands.DeleteFile;
using BackendTemplate.Application.Features.Files.Commands.UploadFile;
using BackendTemplate.Domain.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Files;

public class FileUploadTests
{
    [Theory]
    [InlineData("avatar.png", "image/png", 1024, true)]
    [InlineData("document.pdf", "application/pdf", 2048, true)]
    [InlineData("data.csv", "text/csv", 512, true)]
    [InlineData("script.exe", "application/x-msdownload", 1024, false)]
    [InlineData("virus.bat", "application/bat", 1024, false)]
    [InlineData("", "image/png", 1024, false)]
    public void UploadFileCommandValidator_ShouldValidateExtensionsCorrectly(
        string fileName, string contentType, long size, bool shouldBeValid)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
        var command = new UploadFileCommand(stream, fileName, contentType, size);
        var validator = new UploadFileCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(shouldBeValid);
    }

    [Fact]
    public async Task UploadFileCommandHandler_ShouldCallStorageService()
    {
        // Arrange
        var mockStorage = new Mock<IFileStorageService>();
        var expectedResult = new FileUploadResult(
            FileUrl: "https://res.cloudinary.com/demo/image/upload/sample.jpg",
            PublicId: "sample",
            FileName: "sample.jpg",
            FileSizeBytes: 1024,
            ContentType: "image/jpeg");

        mockStorage.Setup(s => s.UploadFileAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUploadResult>.Success(expectedResult));

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample"));
        var command = new UploadFileCommand(stream, "sample.jpg", "image/jpeg", 1024);
        var handler = new UploadFileCommandHandler(mockStorage.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileUrl.Should().Be(expectedResult.FileUrl);
        result.Value.PublicId.Should().Be("sample");
    }

    [Fact]
    public async Task DeleteFileCommandHandler_ShouldCallStorageService()
    {
        // Arrange
        var mockStorage = new Mock<IFileStorageService>();
        mockStorage.Setup(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteFileCommand("sample_public_id");
        var handler = new DeleteFileCommandHandler(mockStorage.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockStorage.Verify(s => s.DeleteFileAsync("sample_public_id", It.IsAny<CancellationToken>()), Times.Once);
    }
}
