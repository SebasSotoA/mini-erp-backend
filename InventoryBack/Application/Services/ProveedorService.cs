using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class ProveedorService : IProveedorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProveedorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ProveedorDto> CreateAsync(CreateProveedorDto dto, CancellationToken ct = default)
    {
        // Validate: Identificacion must be unique
        var identificacionExists = await _unitOfWork.Proveedores.IdentificacionExistsAsync(dto.Identificacion, null, ct);
        if (identificacionExists)
        {
            throw new BusinessRuleException($"Ya existe un proveedor con la identificación '{dto.Identificacion}'.");
        }

        // ? FIX: Initialize required properties properly
        var proveedor = new Proveedor
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Identificacion = dto.Identificacion.Trim(),
            Correo = dto.Correo?.Trim(),
            Observaciones = dto.Observaciones?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Proveedores.AddAsync(proveedor, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<ProveedorDto>(proveedor);
    }

    public async Task<ProveedorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id, ct);
        return proveedor != null ? _mapper.Map<ProveedorDto>(proveedor) : null;
    }

    public async Task<IEnumerable<ProveedorDto>> GetAllAsync(bool? soloActivos = null, CancellationToken ct = default)
    {
        IEnumerable<Proveedor> proveedores;

        if (soloActivos.HasValue)
        {
            proveedores = await _unitOfWork.Proveedores.ListAsync(
                filter: p => p.Activo == soloActivos.Value,
                ct: ct
            );
        }
        else
        {
            proveedores = await _unitOfWork.Proveedores.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);
    }

    public async Task<PagedResult<ProveedorDto>> GetPagedAsync(ProveedorFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Proveedores.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<ProveedorDto>>(items);

        return new PagedResult<ProveedorDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateProveedorDto dto, CancellationToken ct = default)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id, ct);
        if (proveedor == null)
        {
            throw new NotFoundException("Proveedor", id);
        }

        // Validate: Identificacion must be unique (excluding current proveedor)
        if (dto.Identificacion != proveedor.Identificacion)
        {
            var identificacionExists = await _unitOfWork.Proveedores.IdentificacionExistsAsync(dto.Identificacion, id, ct);
            if (identificacionExists)
            {
                throw new BusinessRuleException($"Ya existe otro proveedor con la identificación '{dto.Identificacion}'.");
            }
        }

        proveedor.Nombre = dto.Nombre.Trim();
        proveedor.Identificacion = dto.Identificacion.Trim();
        proveedor.Correo = dto.Correo?.Trim();
        proveedor.Observaciones = dto.Observaciones?.Trim();

        _unitOfWork.Proveedores.Update(proveedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id, ct);
        if (proveedor == null)
        {
            throw new NotFoundException("Proveedor", id);
        }

        if (proveedor.Activo)
        {
            throw new BusinessRuleException("El proveedor ya está activo.");
        }

        proveedor.Activo = true;
        _unitOfWork.Proveedores.Update(proveedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(id, ct);
        if (proveedor == null)
        {
            throw new NotFoundException("Proveedor", id);
        }

        if (!proveedor.Activo)
        {
            throw new BusinessRuleException("El proveedor ya está inactivo.");
        }

        // Validate: Cannot deactivate if has purchase invoices
        var hasPurchaseInvoices = await _unitOfWork.Proveedores.HasPurchaseInvoicesAsync(id, ct);
        if (hasPurchaseInvoices)
        {
            throw new BusinessRuleException(
                $"No se puede desactivar el proveedor '{proveedor.Nombre}' porque tiene facturas de compra registradas. " +
                "Los proveedores con historial contable deben mantenerse activos para preservar la trazabilidad.");
        }

        proveedor.Activo = false;
        _unitOfWork.Proveedores.Update(proveedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
