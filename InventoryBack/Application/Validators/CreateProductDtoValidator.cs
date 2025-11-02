using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

/// <summary>
/// Validator for CreateProductDto using FluentValidation.
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
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

    public CreateProductDtoValidator()
    {
        // ========== REQUIRED FIELDS ==========
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es requerida.")
            .MaximumLength(50).WithMessage("La unidad de medida no puede exceder 50 caracteres.")
            .Must(unit => ValidUnits.Contains(unit))
            .WithMessage($"La unidad de medida debe ser una de las siguientes: {string.Join(", ", ValidUnits)}");

        RuleFor(x => x.BodegaPrincipalId)
            .NotEmpty().WithMessage("La bodega principal es requerida.");

        RuleFor(x => x.PrecioBase)
            .GreaterThan(0).WithMessage("El precio base debe ser mayor a 0.");

        RuleFor(x => x.CostoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El costo inicial no puede ser negativo.");

        RuleFor(x => x)
            .Must(dto => dto.PrecioBase > dto.CostoInicial)
            .WithMessage("El precio base debe ser mayor que el costo inicial.")
            .When(x => x.CostoInicial > 0);

        RuleFor(x => x.CantidadInicial)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad inicial no puede ser negativa.");

        // ========== OPTIONAL FIELDS ==========

        RuleFor(x => x.ImpuestoPorcentaje)
            .InclusiveBetween(0, 100).WithMessage("El porcentaje de impuesto debe estar entre 0 y 100.")
            .When(x => x.ImpuestoPorcentaje.HasValue);

        RuleFor(x => x.CodigoSku)
            .MaximumLength(30).WithMessage("El código SKU no puede exceder 30 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CodigoSku));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Descripcion));

        // ========== IMAGEN VALIDATION ==========

        RuleFor(x => x.ImagenProductoUrl)
            .MaximumLength(500).WithMessage("La URL de la imagen no puede exceder 500 caracteres.")
            .Must(BeAValidUrl).WithMessage("La URL de la imagen debe ser válida.")
            .When(x => !string.IsNullOrEmpty(x.ImagenProductoUrl));

        // ========== ADDITIONAL WAREHOUSES VALIDATION ==========

        RuleFor(x => x.BodegasAdicionales)
            .Must(NotContainDuplicateBodegas).WithMessage("No se pueden agregar bodegas duplicadas.")
            .Must(NotContainEmptyBodegas).WithMessage("Las bodegas adicionales no pueden tener IDs vacíos.")
            .When(x => x.BodegasAdicionales != null && x.BodegasAdicionales.Any());

        RuleForEach(x => x.BodegasAdicionales)
            .SetValidator(new ProductoBodegaDtoValidator())
            .When(x => x.BodegasAdicionales != null);

        // ========== EXTRA FIELDS VALIDATION ==========

        RuleFor(x => x.CamposExtra)
            .Must(NotContainDuplicateCampoExtraIds).WithMessage("No se pueden agregar campos extra duplicados.")
            .Must(NotContainEmptyCamposExtra).WithMessage("Los campos extra no pueden tener IDs vacíos.")
            .When(x => x.CamposExtra != null && x.CamposExtra.Any());

        RuleForEach(x => x.CamposExtra)
            .SetValidator(new ProductoCampoExtraDtoValidator())
            .When(x => x.CamposExtra != null);
    }

    private bool NotContainDuplicateBodegas(List<ProductoBodegaDto>? bodegas)
    {
        if (bodegas == null || !bodegas.Any()) return true;

        var bodegaIds = bodegas.Select(b => b.BodegaId).ToList();
        return bodegaIds.Count == bodegaIds.Distinct().Count();
    }

    private bool NotContainEmptyBodegas(List<ProductoBodegaDto>? bodegas)
    {
        if (bodegas == null || !bodegas.Any()) return true;

        return !bodegas.Any(b => b.BodegaId == Guid.Empty);
    }

    private bool NotContainDuplicateCampoExtraIds(List<ProductoCampoExtraDto>? campos)
    {
        if (campos == null || !campos.Any()) return true;

        var ids = campos.Select(c => c.CampoExtraId).ToList();
        return ids.Count == ids.Distinct().Count();
    }

    private bool NotContainEmptyCamposExtra(List<ProductoCampoExtraDto>? campos)
    {
        if (campos == null || !campos.Any()) return true;

        return !campos.Any(c => c.CampoExtraId == Guid.Empty);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for ProductoBodegaDto.
/// </summary>
public class ProductoBodegaDtoValidator : AbstractValidator<ProductoBodegaDto>
{
    public ProductoBodegaDtoValidator()
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
            .GreaterThan(x => x.CantidadMinima ?? 0).WithMessage("La cantidad máxima debe ser mayor que la cantidad mínima.")
            .When(x => x.CantidadMaxima.HasValue && x.CantidadMinima.HasValue);
    }
}

/// <summary>
/// Validator for ProductoCampoExtraDto.
/// </summary>
public class ProductoCampoExtraDtoValidator : AbstractValidator<ProductoCampoExtraDto>
{
    public ProductoCampoExtraDtoValidator()
    {
        RuleFor(x => x.CampoExtraId)
            .NotEmpty().WithMessage("El ID del campo extra es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID del campo extra no puede ser vacío.");

        RuleFor(x => x.Valor)
            .MaximumLength(500).WithMessage("El valor del campo no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Valor));
    }
}
