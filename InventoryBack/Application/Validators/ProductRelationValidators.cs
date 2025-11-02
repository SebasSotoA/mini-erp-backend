using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class AddProductoBodegaDtoValidator : AbstractValidator<AddProductoBodegaDto>
{
    public AddProductoBodegaDtoValidator()
    {
        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("El ID de la bodega es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID de la bodega no puede ser vacío.");

        RuleFor(x => x.CantidadInicial)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial no puede ser negativa.");

        RuleFor(x => x.CantidadMinima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima no puede ser negativa.")
            .When(x => x.CantidadMinima.HasValue);

        RuleFor(x => x.CantidadMaxima)
            .GreaterThan(x => x.CantidadMinima ?? 0)
            .WithMessage("La cantidad máxima debe ser mayor que la cantidad mínima.")
            .When(x => x.CantidadMaxima.HasValue && x.CantidadMinima.HasValue);
    }
}

public class UpdateProductoBodegaDtoValidator : AbstractValidator<UpdateProductoBodegaDto>
{
    public UpdateProductoBodegaDtoValidator()
    {
        RuleFor(x => x.CantidadInicial)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial no puede ser negativa.");

        RuleFor(x => x.CantidadMinima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima no puede ser negativa.")
            .When(x => x.CantidadMinima.HasValue);

        RuleFor(x => x.CantidadMaxima)
            .GreaterThan(x => x.CantidadMinima ?? 0)
            .WithMessage("La cantidad máxima debe ser mayor que la cantidad mínima.")
            .When(x => x.CantidadMaxima.HasValue && x.CantidadMinima.HasValue);
    }
}

public class SetProductoCampoExtraDtoValidator : AbstractValidator<SetProductoCampoExtraDto>
{
    public SetProductoCampoExtraDtoValidator()
    {
        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor es requerido.")
            .MaximumLength(500).WithMessage("El valor no puede exceder 500 caracteres.");
    }
}
