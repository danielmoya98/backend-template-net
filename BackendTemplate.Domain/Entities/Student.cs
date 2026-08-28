using BackendTemplate.Domain.Common;

namespace BackendTemplate.Domain.Entities;

public class Student : AuditableEntity, ISoftDeletable
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Soft delete properties
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    private Student() { }

    public Student(string firstName, string lastName, string email, string? phoneNumber = null, DateTime? dateOfBirth = null)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        IsActive = true;
        IsDeleted = false;
    }

    public void Update(string firstName, string lastName, string email, string? phoneNumber, DateTime? dateOfBirth, bool isActive)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        IsActive = isActive;
    }

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
