using FluentValidation;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Validators;

/// <summary>
/// Validator for adding a product to a warehouse.
/// </summary>
public class AddProductoBodegaDtoValidator : AbstractValidator<AddProductoBodegaDto>
{
    public AddProductoBodegaDtoValidator()
    {
        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("El ID de la bodega es requerido.");

        RuleFor(x => x.CantidadInicial)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial debe ser mayor o igual a 0.");

        RuleFor(x => x.CantidadMinima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima debe ser mayor o igual a 0.")
            .When(x => x.CantidadMinima.HasValue);

        RuleFor(x => x.CantidadMaxima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad máxima debe ser mayor o igual a 0.")
            .When(x => x.CantidadMaxima.HasValue);

        // Validar que CantidadMinima < CantidadMaxima
        RuleFor(x => x)
            .Must(x => !x.CantidadMinima.HasValue || !x.CantidadMaxima.HasValue || x.CantidadMinima.Value < x.CantidadMaxima.Value)
            .WithMessage("La cantidad mínima debe ser menor que la cantidad máxima.")
            .When(x => x.CantidadMinima.HasValue && x.CantidadMaxima.HasValue);
    }
}

/// <summary>
/// Validator for updating product quantities in a warehouse.
/// </summary>
public class UpdateProductoBodegaDtoValidator : AbstractValidator<UpdateProductoBodegaDto>
{
    public UpdateProductoBodegaDtoValidator()
    {
        RuleFor(x => x.CantidadInicial)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial debe ser mayor o igual a 0.");

        RuleFor(x => x.CantidadMinima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima debe ser mayor o igual a 0.")
            .When(x => x.CantidadMinima.HasValue);

        RuleFor(x => x.CantidadMaxima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad máxima debe ser mayor o igual a 0.")
            .When(x => x.CantidadMaxima.HasValue);

        // Validar que CantidadMinima < CantidadMaxima
        RuleFor(x => x)
            .Must(x => !x.CantidadMinima.HasValue || !x.CantidadMaxima.HasValue || x.CantidadMinima.Value < x.CantidadMaxima.Value)
            .WithMessage("La cantidad mínima debe ser menor que la cantidad máxima.")
            .When(x => x.CantidadMinima.HasValue && x.CantidadMaxima.HasValue);
    }
}
