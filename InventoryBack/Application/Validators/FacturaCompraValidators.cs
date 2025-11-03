using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class CreateFacturaCompraDtoValidator : AbstractValidator<CreateFacturaCompraDto>
{
    public CreateFacturaCompraDtoValidator()
    {
        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("La bodega es requerida.")
            .NotEqual(Guid.Empty).WithMessage("El ID de la bodega no puede ser vacío.");

        RuleFor(x => x.ProveedorId)
            .NotEmpty().WithMessage("El proveedor es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID del proveedor no puede ser vacío.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La factura debe tener al menos un item.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("La factura debe tener al menos un item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateFacturaCompraDetalleDtoValidator());
    }
}

public class CreateFacturaCompraDetalleDtoValidator : AbstractValidator<CreateFacturaCompraDetalleDto>
{
    public CreateFacturaCompraDetalleDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El producto es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID del producto no puede ser vacío.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

        RuleFor(x => x.CostoUnitario)
            .GreaterThan(0).WithMessage("El costo unitario debe ser mayor a 0.");

        RuleFor(x => x.Descuento)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.")
            .When(x => x.Descuento.HasValue);

        RuleFor(x => x.Impuesto)
            .GreaterThanOrEqualTo(0).WithMessage("El impuesto no puede ser negativo.")
            .When(x => x.Impuesto.HasValue);
    }
}
