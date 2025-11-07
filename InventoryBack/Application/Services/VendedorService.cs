using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class VendedorService : IVendedorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VendedorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VendedorDto> CreateAsync(CreateVendedorDto dto, CancellationToken ct = default)
    {
        // Validate: Identificacion must be unique
        var identificacionExists = await _unitOfWork.Vendedores.IdentificacionExistsAsync(dto.Identificacion, null, ct);
        if (identificacionExists)
        {
            throw new BusinessRuleException($"Ya existe un vendedor con la identificación '{dto.Identificacion}'.");
        }

        var vendedor = new Vendedor
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Identificacion = dto.Identificacion.Trim(),
            Correo = dto.Correo?.Trim(),
            Observaciones = dto.Observaciones?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Vendedores.AddAsync(vendedor, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<VendedorDto>(vendedor);
    }

    public async Task<VendedorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(id, ct);
        return vendedor != null ? _mapper.Map<VendedorDto>(vendedor) : null;
    }

    public async Task<IEnumerable<VendedorDto>> GetAllAsync(bool? soloActivos = null, CancellationToken ct = default)
    {
        IEnumerable<Vendedor> vendedores;

        if (soloActivos.HasValue)
        {
            vendedores = await _unitOfWork.Vendedores.ListAsync(
                filter: v => v.Activo == soloActivos.Value,
                ct: ct
            );
        }
        else
        {
            vendedores = await _unitOfWork.Vendedores.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<VendedorDto>>(vendedores);
    }

    public async Task<PagedResult<VendedorDto>> GetPagedAsync(VendedorFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.Vendedores.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<VendedorDto>>(items);

        return new PagedResult<VendedorDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateVendedorDto dto, CancellationToken ct = default)
    {
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(id, ct);
        if (vendedor == null)
        {
            throw new NotFoundException("Vendedor", id);
        }

        // Validate: Identificacion must be unique (excluding current vendedor)
        if (dto.Identificacion != vendedor.Identificacion)
        {
            var identificacionExists = await _unitOfWork.Vendedores.IdentificacionExistsAsync(dto.Identificacion, id, ct);
            if (identificacionExists)
            {
                throw new BusinessRuleException($"Ya existe otro vendedor con la identificación '{dto.Identificacion}'.");
            }
        }

        vendedor.Nombre = dto.Nombre.Trim();
        vendedor.Identificacion = dto.Identificacion.Trim();
        vendedor.Correo = dto.Correo?.Trim();
        vendedor.Observaciones = dto.Observaciones?.Trim();

        _unitOfWork.Vendedores.Update(vendedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(id, ct);
        if (vendedor == null)
        {
            throw new NotFoundException("Vendedor", id);
        }

        if (vendedor.Activo)
        {
            throw new BusinessRuleException("El vendedor ya está activo.");
        }

        vendedor.Activo = true;
        _unitOfWork.Vendedores.Update(vendedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var vendedor = await _unitOfWork.Vendedores.GetByIdAsync(id, ct);
        if (vendedor == null)
        {
            throw new NotFoundException("Vendedor", id);
        }

        if (!vendedor.Activo)
        {
            throw new BusinessRuleException("El vendedor ya está inactivo.");
        }

        // Validate: Cannot deactivate if has sales invoices
        var hasSalesInvoices = await _unitOfWork.Vendedores.HasSalesInvoicesAsync(id, ct);
        if (hasSalesInvoices)
        {
            throw new BusinessRuleException(
                $"No se puede desactivar el vendedor '{vendedor.Nombre}' porque tiene facturas de venta registradas. " +
                "Los vendedores con historial de ventas deben mantenerse activos para preservar la trazabilidad.");
        }

        vendedor.Activo = false;
        _unitOfWork.Vendedores.Update(vendedor);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
