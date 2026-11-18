using FluentValidation;
using SpendAudit.Application.Features.Politicas.DTOs;

namespace SpendAudit.Application.Features.Politicas.Validators;

public class TopePuestoValidator : AbstractValidator<TopePuestoDto>
{
    public TopePuestoValidator()
    {
        RuleFor(x => x.PuestoId)
            .NotEmpty().WithMessage("El puesto es obligatorio.");

        RuleFor(x => x.TopeDiarioHNL)
            .GreaterThan(0m).WithMessage("El tope diario debe ser mayor que cero.")
            .LessThanOrEqualTo(10_000_000m).WithMessage("El tope diario es demasiado alto.");
    }
}

public class UpdatePoliticaRequestValidator : AbstractValidator<UpdatePoliticaRequestDto>
{
    public UpdatePoliticaRequestValidator()
    {
        RuleFor(x => x.DiasVigenciaMaxima)
            .InclusiveBetween(1, 365).WithMessage("La vigencia debe estar entre 1 y 365 días.");

        RuleFor(x => x.Topes)
            .NotNull().WithMessage("La lista de topes es obligatoria (puede ir vacía).")
            .Must(t => t == null || t.Select(x => x.PuestoId).Distinct().Count() == t.Count)
            .WithMessage("Hay más de un tope para el mismo puesto.");
        RuleForEach(x => x.Topes).SetValidator(new TopePuestoValidator());

        RuleFor(x => x.TerminosRestringidos)
            .NotNull().WithMessage("La lista de términos es obligatoria (puede ir vacía).")
            .Must(t => t == null || t.Count <= 200).WithMessage("Máximo 200 términos restringidos.");
        RuleForEach(x => x.TerminosRestringidos)
            .NotEmpty().WithMessage("Un término restringido no puede estar vacío.")
            .MaximumLength(60).WithMessage("Un término no puede exceder 60 caracteres.");
    }
}
