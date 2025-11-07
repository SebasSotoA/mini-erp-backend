using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class CreateFacturaVentaDtoValidator : AbstractValidator<CreateFacturaVentaDto>
{
    public CreateFacturaVentaDtoValidator()
    {
        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("La bodega es requerida.");

        RuleFor(x => x.VendedorId)
            .NotEmpty().WithMessage("El vendedor es requerido.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.FormaPago)
            .NotEmpty().WithMessage("La forma de pago es requerida.")
            .MaximumLength(50).WithMessage("La forma de pago no puede exceder 50 caracteres.")
            .Must(fp => new[] { "Contado", "Credito" }.Contains(fp))
            .WithMessage("La forma de pago debe ser 'Contado' o 'Credito'.");

        RuleFor(x => x.MedioPago)
            .NotEmpty().WithMessage("El medio de pago es requerido.")
            .MaximumLength(50).WithMessage("El medio de pago no puede exceder 50 caracteres.")
            .Must(mp => new[] { "Efectivo", "Tarjeta", "Transferencia", "Cheque" }.Contains(mp))
            .WithMessage("El medio de pago debe ser 'Efectivo', 'Tarjeta', 'Transferencia' o 'Cheque'.");

        RuleFor(x => x.PlazoPago)
            .GreaterThan(0).WithMessage("El plazo de pago debe ser mayor a 0 días.")
            .When(x => x.FormaPago == "Credito" && x.PlazoPago.HasValue);

        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0).WithMessage("El total no puede ser negativo.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe incluir al menos un item en la factura.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("La factura debe tener al menos un item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateFacturaVentaDetalleDtoValidator());
    }
}

public class CreateFacturaVentaDetalleDtoValidator : AbstractValidator<CreateFacturaVentaDetalleDto>
{
    public CreateFacturaVentaDetalleDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El producto es requerido.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

        RuleFor(x => x.PrecioUnitario)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0.");

        RuleFor(x => x.Descuento)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.")
            .When(x => x.Descuento.HasValue);

        RuleFor(x => x.Impuesto)
            .GreaterThanOrEqualTo(0).WithMessage("El impuesto no puede ser negativo.")
            .When(x => x.Impuesto.HasValue);
    }
}
