using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class CampoExtraService : ICampoExtraService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CampoExtraService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CampoExtraDto> CreateAsync(CreateCampoExtraDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingCampo = await _unitOfWork.CamposExtras.GetByNameAsync(dto.Nombre, ct);
        if (existingCampo != null)
        {
            throw new BusinessRuleException($"Ya existe un campo extra con el nombre '{dto.Nombre}'.");
        }

        var campoExtra = new CampoExtra
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            TipoDato = dto.TipoDato.Trim(),
            EsRequerido = dto.EsRequerido,
            ValorPorDefecto = dto.ValorPorDefecto?.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.CamposExtras.AddAsync(campoExtra, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CampoExtraDto>(campoExtra);
    }

    public async Task<CampoExtraDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        return campoExtra != null ? _mapper.Map<CampoExtraDto>(campoExtra) : null;
    }

    public async Task<IEnumerable<CampoExtraDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<CampoExtra> camposExtra;

        if (activo.HasValue)
        {
            camposExtra = await _unitOfWork.CamposExtras.ListAsync(
                filter: ce => ce.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            camposExtra = await _unitOfWork.CamposExtras.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<CampoExtraDto>>(camposExtra);
    }

    public async Task<PagedResult<CampoExtraDto>> GetPagedAsync(CampoExtraFilterDto filters, CancellationToken ct = default)
    {
        var (items, totalCount) = await _unitOfWork.CamposExtras.GetPagedAsync(filters, ct);

        var dtos = _mapper.Map<IEnumerable<CampoExtraDto>>(items);

        return new PagedResult<CampoExtraDto>
        {
            Items = dtos,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task UpdateAsync(Guid id, UpdateCampoExtraDto dto, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        // Validate name uniqueness (excluding current campo)
        var existingCampo = await _unitOfWork.CamposExtras.GetByNameAsync(dto.Nombre, ct);
        if (existingCampo != null && existingCampo.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otro campo extra con el nombre '{dto.Nombre}'.");
        }

        campoExtra.Nombre = dto.Nombre.Trim();
        campoExtra.TipoDato = dto.TipoDato.Trim();
        campoExtra.EsRequerido = dto.EsRequerido;
        campoExtra.ValorPorDefecto = dto.ValorPorDefecto?.Trim();
        campoExtra.Descripcion = dto.Descripcion?.Trim();

        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        if (campoExtra.Activo)
        {
            throw new BusinessRuleException("El campo extra ya está activo.");
        }

        campoExtra.Activo = true;
        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        if (!campoExtra.Activo)
        {
            throw new BusinessRuleException("El campo extra ya está desactivado.");
        }

        campoExtra.Activo = false;
        _unitOfWork.CamposExtras.Update(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var campoExtra = await _unitOfWork.CamposExtras.GetByIdAsync(id, ct);
        if (campoExtra == null)
        {
            throw new NotFoundException("CampoExtra", id);
        }

        // Check if campo is being used in products
        var isInUse = await _unitOfWork.ProductoCamposExtras.CountAsync(
            filter: pce => pce.CampoExtraId == id,
            ct: ct);

        if (isInUse > 0)
        {
            throw new BusinessRuleException(
                "No se puede eliminar el campo extra porque está siendo utilizado en productos. " +
                "Considere desactivarlo en su lugar.");
        }

        _unitOfWork.CamposExtras.Remove(campoExtra);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
