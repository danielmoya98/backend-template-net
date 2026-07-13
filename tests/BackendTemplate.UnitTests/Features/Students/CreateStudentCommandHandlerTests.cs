using BackendTemplate.Application.Features.Students.Commands;
using FluentAssertions;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Students;

public class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithGuid_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateStudentCommand("Daniel", "Moya", "daniel@test.com");
        var handler = new CreateStudentCommandHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty(); // Garantiza que devolvió un Guid válido
    }
}
