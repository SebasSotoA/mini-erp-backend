using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

/// <summary>
/// Validator for CreateProductDto using FluentValidation.
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        // ========== REQUIRED FIELDS ==========
        
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es requerida.")
            .MaximumLength(50).WithMessage("La unidad de medida no puede exceder 50 caracteres.");

        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("La bodega principal es requerida.");

        RuleFor(x => x.PrecioBase)
            .GreaterThan(0).WithMessage("El precio base debe ser mayor a 0.");

        RuleFor(x => x.CostoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El costo inicial no puede ser negativo.");

        RuleFor(x => x)
            .Must(dto => dto.PrecioBase > dto.CostoInicial)
            .WithMessage("El precio base debe ser mayor que el costo inicial.")
            .When(x => x.CostoInicial > 0);

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");

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

        RuleFor(x => x.Imagen)
            .Must(BeAValidImage).WithMessage("El archivo debe ser una imagen válida (jpg, jpeg, png, gif, webp).")
            .When(x => x.Imagen != null);

        RuleFor(x => x.Imagen)
            .Must(BeUnder5MB).WithMessage("El tamaño de la imagen no puede exceder 5 MB.")
            .When(x => x.Imagen != null);

        // ========== ADDITIONAL WAREHOUSES VALIDATION ==========

        RuleFor(x => x.BodegasAdicionales)
            .Must(NotContainDuplicateBodegas).WithMessage("No se pueden agregar bodegas duplicadas.")
            .When(x => x.BodegasAdicionales != null && x.BodegasAdicionales.Any());

        RuleForEach(x => x.BodegasAdicionales)
            .SetValidator(new ProductoBodegaDtoValidator())
            .When(x => x.BodegasAdicionales != null);

        // ========== ADDITIONAL FIELDS VALIDATION ==========

        RuleFor(x => x.CamposAdicionales)
            .Must(NotContainDuplicateFieldNames).WithMessage("No se pueden agregar campos adicionales con nombres duplicados.")
            .When(x => x.CamposAdicionales != null && x.CamposAdicionales.Any());

        RuleForEach(x => x.CamposAdicionales)
            .SetValidator(new CampoAdicionalDtoValidator())
            .When(x => x.CamposAdicionales != null);
    }

    private bool BeAValidImage(IFormFile? file)
    {
        if (file == null) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    private bool BeUnder5MB(IFormFile? file)
    {
        if (file == null) return true;

        const long maxSizeInBytes = 5 * 1024 * 1024; // 5 MB
        return file.Length <= maxSizeInBytes;
    }

    private bool NotContainDuplicateBodegas(List<ProductoBodegaDto>? bodegas)
    {
        if (bodegas == null || !bodegas.Any()) return true;

        var bodegaIds = bodegas.Select(b => b.BodegaId).ToList();
        return bodegaIds.Count == bodegaIds.Distinct().Count();
    }

    private bool NotContainDuplicateFieldNames(List<CampoAdicionalDto>? campos)
    {
        if (campos == null || !campos.Any()) return true;

        var nombres = campos.Select(c => c.Nombre.Trim().ToLower()).ToList();
        return nombres.Count == nombres.Distinct().Count();
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
            .NotEmpty().WithMessage("El ID de la bodega es requerido.");

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");

        RuleFor(x => x.CantidadMinima)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad mínima no puede ser negativa.")
            .When(x => x.CantidadMinima.HasValue);

        RuleFor(x => x.CantidadMaxima)
            .GreaterThan(x => x.CantidadMinima ?? 0).WithMessage("La cantidad máxima debe ser mayor que la cantidad mínima.")
            .When(x => x.CantidadMaxima.HasValue && x.CantidadMinima.HasValue);
    }
}

/// <summary>
/// Validator for CampoAdicionalDto.
/// </summary>
public class CampoAdicionalDtoValidator : AbstractValidator<CampoAdicionalDto>
{
    public CampoAdicionalDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del campo adicional es requerido.")
            .MaximumLength(100).WithMessage("El nombre del campo no puede exceder 100 caracteres.");

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor del campo adicional es requerido.")
            .MaximumLength(500).WithMessage("El valor del campo no puede exceder 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Valor));
    }
}
