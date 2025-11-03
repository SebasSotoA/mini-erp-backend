using FluentValidation;
using InventoryBack.Application.DTOs;

namespace InventoryBack.Application.Validators;

public class CreateFacturaVentaDtoValidator : AbstractValidator<CreateFacturaVentaDto>
{
    private static readonly string[] ValidFormasPago = { "Contado", "Crédito" };
    private static readonly string[] ValidMediosPago = 
    {
        "Efectivo",
        "Tarjeta de Crédito",
        "Tarjeta de Débito",
        "Transferencia Bancaria",
        "Cheque",
        "PSE",
        "Nequi",
        "Daviplata"
    };

    public CreateFacturaVentaDtoValidator()
    {
        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("La bodega es requerida.")
            .NotEqual(Guid.Empty).WithMessage("El ID de la bodega no puede ser vacío.");

        RuleFor(x => x.VendedorId)
            .NotEmpty().WithMessage("El vendedor es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID del vendedor no puede ser vacío.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.FormaPago)
            .NotEmpty().WithMessage("La forma de pago es requerida.")
            .Must(fp => ValidFormasPago.Contains(fp)).WithMessage("La forma de pago debe ser 'Contado' o 'Crédito'.");

        RuleFor(x => x.PlazoPago)
            .GreaterThan(0).WithMessage("El plazo de pago debe ser mayor a 0.")
            .When(x => x.FormaPago == "Crédito")
            .WithMessage("El plazo de pago es requerido cuando la forma de pago es 'Crédito'.");

        RuleFor(x => x.PlazoPago)
            .Null().WithMessage("El plazo de pago solo aplica para forma de pago 'Crédito'.")
            .When(x => x.FormaPago == "Contado");

        RuleFor(x => x.MedioPago)
            .NotEmpty().WithMessage("El medio de pago es requerido.")
            .Must(mp => ValidMediosPago.Contains(mp))
            .WithMessage($"El medio de pago debe ser uno de: {string.Join(", ", ValidMediosPago)}");

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000).WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La factura debe tener al menos un item.")
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
            .NotEmpty().WithMessage("El producto es requerido.")
            .NotEqual(Guid.Empty).WithMessage("El ID del producto no puede ser vacío.");

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
