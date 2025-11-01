using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class BodegaService : IBodegaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BodegaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BodegaDto> CreateAsync(CreateBodegaDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingBodega = await _unitOfWork.Bodegas.GetByNameAsync(dto.Nombre, ct);
        if (existingBodega != null)
        {
            throw new BusinessRuleException($"Ya existe una bodega con el nombre '{dto.Nombre}'.");
        }

        var bodega = new Bodega
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Bodegas.AddAsync(bodega, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<BodegaDto>(bodega);
    }

    public async Task<BodegaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        return bodega != null ? _mapper.Map<BodegaDto>(bodega) : null;
    }

    public async Task<IEnumerable<BodegaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<Bodega> bodegas;

        if (activo.HasValue)
        {
            bodegas = await _unitOfWork.Bodegas.ListAsync(
                filter: b => b.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            bodegas = await _unitOfWork.Bodegas.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<BodegaDto>>(bodegas);
    }

    public async Task UpdateAsync(Guid id, UpdateBodegaDto dto, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        // Validate name uniqueness (excluding current bodega)
        var existingBodega = await _unitOfWork.Bodegas.GetByNameAsync(dto.Nombre, ct);
        if (existingBodega != null && existingBodega.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otra bodega con el nombre '{dto.Nombre}'.");
        }

        bodega.Nombre = dto.Nombre.Trim();
        bodega.Direccion = dto.Direccion?.Trim();
        bodega.Descripcion = dto.Descripcion?.Trim();

        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        if (bodega.Activo)
        {
            throw new BusinessRuleException("La bodega ya está activa.");
        }

        bodega.Activo = true;
        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        if (!bodega.Activo)
        {
            throw new BusinessRuleException("La bodega ya está desactivada.");
        }

        // Check if bodega has products
        var hasProducts = await _unitOfWork.Bodegas.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede desactivar la bodega porque tiene productos asignados. " +
                "Reasigne los productos a otra bodega antes de desactivarla.");
        }

        bodega.Activo = false;
        _unitOfWork.Bodegas.Update(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var bodega = await _unitOfWork.Bodegas.GetByIdAsync(id, ct);
        if (bodega == null)
        {
            throw new NotFoundException("Bodega", id);
        }

        // Check if bodega has products
        var hasProducts = await _unitOfWork.Bodegas.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene productos asignados. " +
                "Elimine o reasigne todos los productos antes de eliminar la bodega.");
        }

        // Check if bodega has invoices
        var hasInvoices = await _unitOfWork.Bodegas.HasInvoicesAsync(id, ct);
        if (hasInvoices)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene facturas asociadas. " +
                "Considere desactivarla en su lugar.");
        }

        // Check if bodega has inventory movements
        var hasMovements = await _unitOfWork.Bodegas.HasMovementsAsync(id, ct);
        if (hasMovements)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la bodega porque tiene movimientos de inventario registrados. " +
                "Considere desactivarla en su lugar.");
        }

        _unitOfWork.Bodegas.Remove(bodega);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
