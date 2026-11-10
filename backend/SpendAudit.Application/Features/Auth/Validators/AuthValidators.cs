using FluentValidation;
using SpendAudit.Application.Features.Auth.DTOs;

namespace SpendAudit.Application.Features.Auth.Validators;

public class RegistrarEmpresaRequestValidator : AbstractValidator<RegistrarEmpresaRequestDto>
{
    public RegistrarEmpresaRequestValidator()
    {
        RuleFor(x => x.NombreRazonSocial)
            .NotEmpty().WithMessage("El nombre de la empresa es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.RTN)
            .NotEmpty().WithMessage("El RTN es obligatorio.")
            .Matches(@"^\d{14}$").WithMessage("El RTN debe tener 14 dígitos, sin guiones.");

        RuleFor(x => x.AdminNombre)
            .NotEmpty().WithMessage("El nombre del administrador es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .MaximumLength(200).WithMessage("El correo no puede exceder 200 caracteres.");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Za-z]").WithMessage("La contraseña debe incluir al menos una letra.")
            .Matches("[0-9]").WithMessage("La contraseña debe incluir al menos un número.")
            .MaximumLength(72).WithMessage("La contraseña no puede exceder 72 caracteres.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
