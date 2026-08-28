using BackendTemplate.Application.Features.Students.Commands;
using BackendTemplate.Domain.Common;
using BackendTemplate.Domain.Entities;
using BackendTemplate.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Students;

public class StudentCommandHandlerTests
{
    [Fact]
    public async Task DeleteStudentCommandHandler_ShouldReturnNotFoundResult_WhenStudentDoesNotExist()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteStudentCommandHandler(context);
        var command = new DeleteStudentCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteStudentCommandHandler_ShouldPerformSoftDelete_WhenStudentExists()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var student = new Student("Carlos", "Santana", "carlos@test.com");
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        var handler = new DeleteStudentCommandHandler(context);
        var command = new DeleteStudentCommand(student.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStudentCommandHandler_ShouldReturnNotFoundResult_WhenStudentDoesNotExist()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateStudentCommandHandler(context);
        var command = new UpdateStudentCommand(Guid.NewGuid(), "NewName", "NewLastName", "new@example.com", "+1234567890", null, true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateStudentCommandHandler_ShouldUpdateFields_WhenStudentExists()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var student = new Student("Original", "Name", "original@test.com");
        await context.Students.AddAsync(student);
        await context.SaveChangesAsync();

        var handler = new UpdateStudentCommandHandler(context);
        var command = new UpdateStudentCommand(student.Id, "UpdatedFirst", "UpdatedLast", "updated@test.com", "+1 999 888", new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc), true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        student.FirstName.Should().Be("UpdatedFirst");
        student.LastName.Should().Be("UpdatedLast");
        student.Email.Should().Be("updated@test.com");
    }
}
