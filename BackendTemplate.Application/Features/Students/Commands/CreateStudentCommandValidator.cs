using FluentValidation;

namespace BackendTemplate.Application.Features.Students.Commands;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres.");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo no es válido.");
    }
}
