namespace BackendTemplate.Application.Features.Students.DTOs;

public record StudentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    bool IsActive,
    DateTime CreatedAt,
    string? CreatedBy);
