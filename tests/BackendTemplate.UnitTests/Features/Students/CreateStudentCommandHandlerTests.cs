using BackendTemplate.Application.Features.Students.Commands;
using BackendTemplate.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Students;

public class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithGuid_WhenRequestIsValid()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var command = new CreateStudentCommand("Daniel", "Moya", "daniel@test.com", "+1 555-0199");
        var handler = new CreateStudentCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var createdStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == result.Value);
        createdStudent.Should().NotBeNull();
        createdStudent!.FirstName.Should().Be("Daniel");
        createdStudent.LastName.Should().Be("Moya");
        createdStudent.Email.Should().Be("daniel@test.com");
    }
}
