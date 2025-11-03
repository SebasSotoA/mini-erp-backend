using FluentValidation;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;

namespace InventoryBack.Application.Validators;

public class CreateFacturaCompraDtoValidator : AbstractValidator<CreateFacturaCompraDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFacturaCompraDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        RuleFor(x => x.BodegaId)
            .NotEmpty().WithMessage("La bodega es requerida.")
            .MustAsync(BodegaMustExistAndBeActive)
            .WithMessage("La bodega no existe o no está activa.");

        RuleFor(x => x.ProveedorId)
            .NotEmpty().WithMessage("El proveedor es requerido.")
            .MustAsync(ProveedorMustExistAndBeActive)
            .WithMessage("El proveedor no existe o no está activo.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("La fecha no puede ser futura.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe incluir al menos un item en la factura.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("La factura debe tener al menos un item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateFacturaCompraDetalleDtoValidator(_unitOfWork));

        // Validar que el total calculado coincida con la suma de items
        RuleFor(x => x)
            .MustAsync(TotalMustMatchItemsSum)
            .WithMessage("El total de la factura no coincide con la suma de los items.");
    }

    private async Task<bool> BodegaMustExistAndBeActive(Guid bodegaId, CancellationToken ct)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(bodegaId);
        return bodega != null && bodega.Activo;
    }

    private async Task<bool> ProveedorMustExistAndBeActive(Guid proveedorId, CancellationToken ct)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(proveedorId);
        return proveedor != null && proveedor.Activo;
    }

    private async Task<bool> TotalMustMatchItemsSum(CreateFacturaCompraDto dto, CancellationToken ct)
    {
        if (dto.Items == null || !dto.Items.Any())
            return true;

        // Calcular total esperado
        decimal totalCalculado = 0;
        foreach (var item in dto.Items)
        {
            decimal subtotal = item.Cantidad * item.CostoUnitario;
            decimal descuento = item.Descuento ?? 0;
            decimal impuesto = item.Impuesto ?? 0;
            decimal totalLinea = subtotal - descuento + impuesto;
            totalCalculado += totalLinea;
        }

        // Permitir una diferencia de 0.01 por redondeo
        return Math.Abs(totalCalculado - dto.Total) < 0.01m;
    }
}

public class CreateFacturaCompraDetalleDtoValidator : AbstractValidator<CreateFacturaCompraDetalleDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFacturaCompraDetalleDtoValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El producto es requerido.")
            .MustAsync(ProductoMustExistAndBeActive)
            .WithMessage("El producto no existe o no está activo.");

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

    private async Task<bool> ProductoMustExistAndBeActive(Guid productoId, CancellationToken ct)
    {
        var producto = await _unitOfWork.Products.GetByIdAsync(productoId);
        return producto != null && producto.Activo;
    }
}
