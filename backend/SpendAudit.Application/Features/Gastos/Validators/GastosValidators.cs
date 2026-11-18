using FluentValidation;
using SpendAudit.Application.Common.Models;
using SpendAudit.Application.Features.Gastos.DTOs;

namespace SpendAudit.Application.Features.Gastos.Validators;

/// <summary>Reglas comunes de los datos de cabecera (se reutilizan en crear y en editar).</summary>
public abstract class DatosComprobanteValidatorBase<T> : AbstractValidator<T> where T : IDatosComprobanteDto
{
    protected DatosComprobanteValidatorBase()
    {
        RuleFor(x => x.NumeroFactura)
            .MaximumLength(30).WithMessage("El número de factura no puede exceder 30 caracteres.");

        RuleFor(x => x.CAI)
            .Matches(@"^[0-9A-Fa-f]{6}(-[0-9A-Fa-f]{6}){4}-[0-9A-Fa-f]{2}$")
            .WithMessage("El CAI debe tener el formato XXXXXX-XXXXXX-XXXXXX-XXXXXX-XXXXXX-XX.")
            .When(x => !string.IsNullOrWhiteSpace(x.CAI));

        RuleFor(x => x.ProveedorRTN)
            .Matches(@"^\d{14}$").WithMessage("El RTN del proveedor debe tener 14 dígitos, sin guiones.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProveedorRTN));

        RuleFor(x => x.FechaEmision)
            .Must(f => f!.Value <= HoraLocal.Hoy())
            .WithMessage("La fecha de emisión no puede estar en el futuro.")
            .When(x => x.FechaEmision.HasValue);

        RuleFor(x => x.MontoTotal)
            .GreaterThan(0m).WithMessage("El monto total debe ser mayor que cero.")
            .Must(m => decimal.Round(m!.Value, 2) == m)
            .WithMessage("El monto total admite máximo 2 decimales.")
            .When(x => x.MontoTotal.HasValue);

        RuleFor(x => x.ISV)
            .GreaterThanOrEqualTo(0m).WithMessage("El ISV no puede ser negativo.")
            .Must(i => decimal.Round(i!.Value, 2) == i)
            .WithMessage("El ISV admite máximo 2 decimales.")
            .When(x => x.ISV.HasValue);

        RuleFor(x => x)
            .Must(x => x.ISV!.Value <= x.MontoTotal!.Value)
            .OverridePropertyName("ISV")
            .WithMessage("El ISV no puede ser mayor que el monto total.")
            .When(x => x.ISV.HasValue && x.MontoTotal.HasValue);
    }
}

public class CreateBorradorRequestValidator : DatosComprobanteValidatorBase<CreateBorradorRequestDto>
{
    public CreateBorradorRequestValidator()
    {
        RuleFor(x => x.ArchivoNombre)
            .NotEmpty().WithMessage("Debes adjuntar un archivo.")
            .Must(n => LimitesArchivo.ExtensionesPermitidas.Contains(
                Path.GetExtension(n ?? string.Empty).ToLowerInvariant()))
            .WithMessage("Solo se permiten archivos PDF, JPG o PNG.");

        RuleFor(x => x.TextoOcr)
            .MaximumLength(50_000).WithMessage("El texto reconocido de la imagen es demasiado largo.");

        RuleFor(x => x.ArchivoTamano)
            .GreaterThan(0).WithMessage("El archivo está vacío.")
            .LessThanOrEqualTo(LimitesArchivo.TamanoMaximoBytes)
            .WithMessage("El archivo no puede pesar más de 10 MB.");
    }
}

public class UpdateBorradorRequestValidator : DatosComprobanteValidatorBase<UpdateBorradorRequestDto>
{
}

public class LiquidarLoteRequestValidator : AbstractValidator<LiquidarLoteRequestDto>
{
    public LiquidarLoteRequestValidator()
    {
        RuleFor(x => x.ComprobanteIds)
            .NotEmpty().WithMessage("Selecciona al menos un comprobante para liquidar.")
            .Must(ids => ids.Count <= 100).WithMessage("Un lote admite máximo 100 comprobantes.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("El lote tiene comprobantes repetidos.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("El lote contiene un identificador inválido.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres.");
    }
}
