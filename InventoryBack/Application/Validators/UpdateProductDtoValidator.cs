using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

/// <summary>
/// Validator for UpdateProductDto.
/// </summary>
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    private static readonly string[] ValidUnits =
    [
        // Unidad
        "Unidad", "Pieza", "Paquete", "Caja", "Docena",
        // Longitud
        "Metro", "Centímetro", "Kilómetro", "Pulgada", "Pie",
        // Área
        "Metro²", "Centímetro²", "Hectárea",
        // Volumen
        "Litro", "Mililitro", "Metro³", "Galón",
        // Peso
        "Kilogramo", "Gramo", "Tonelada", "Libra", "Onza"
    ];

    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
            .Must(unit => ValidUnits.Contains(unit))
            .WithMessage($"La unidad de medida debe ser una de las siguientes: {string.Join(", ", ValidUnits)}");

        RuleFor(x => x.PrecioBase)
            .GreaterThan(0).WithMessage("El precio base debe ser mayor a 0.");

        RuleFor(x => x.ImpuestoPorcentaje)
            .InclusiveBetween(0, 100).When(x => x.ImpuestoPorcentaje.HasValue)
            .WithMessage("El porcentaje de impuesto debe estar entre 0 y 100.");

        RuleFor(x => x.CostoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El costo inicial no puede ser negativo.");

        RuleFor(x => x.CodigoSku)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.CodigoSku))
            .WithMessage("El código SKU no puede exceder 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Descripcion))
            .WithMessage("La descripción no puede exceder 1000 caracteres.");
    }
}
