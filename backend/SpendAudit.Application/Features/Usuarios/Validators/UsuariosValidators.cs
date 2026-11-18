using FluentValidation;
using SpendAudit.Application.Common.Models;
using SpendAudit.Application.Features.Usuarios.DTOs;

namespace SpendAudit.Application.Features.Usuarios.Validators;

public class CreatePuestoRequestValidator : AbstractValidator<CreatePuestoRequestDto>
{
    public CreatePuestoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del puesto es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del puesto no puede exceder 100 caracteres.");
    }
}

public class CreateUsuarioRequestValidator : AbstractValidator<CreateUsuarioRequestDto>
{
    private static readonly string[] RolesPermitidos = { Roles.Admin, Roles.Empleado, Roles.Auditor };

    public CreateUsuarioRequestValidator()
    {
        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage("El nombre completo es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El correo no puede exceder 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Za-z]").WithMessage("La contraseña debe incluir al menos una letra.")
            .Matches("[0-9]").WithMessage("La contraseña debe incluir al menos un número.")
            .MaximumLength(72).WithMessage("La contraseña no puede exceder 72 caracteres.");

        RuleFor(x => x.Rol)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(rol => RolesPermitidos.Contains(rol.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("El rol debe ser Admin, Empleado o Auditor.");

        RuleFor(x => x.PuestoId)
            .NotEqual(Guid.Empty).WithMessage("El puesto no es válido.")
            .When(x => x.PuestoId.HasValue);
    }
}
