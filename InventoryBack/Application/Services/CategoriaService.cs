using AutoMapper;
using InventoryBack.Application.DTOs;
using InventoryBack.Application.Exceptions;
using InventoryBack.Application.Interfaces;
using InventoryBack.Domain.Entities;

namespace InventoryBack.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CategoriaDto> CreateAsync(CreateCategoriaDto dto, CancellationToken ct = default)
    {
        // Validate: Name uniqueness
        var existingCategoria = await _unitOfWork.Categorias.GetByNameAsync(dto.Nombre, ct);
        if (existingCategoria != null)
        {
            throw new BusinessRuleException($"Ya existe una categoría con el nombre '{dto.Nombre}'.");
        }

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim(),
            ImagenCategoriaUrl = dto.ImagenCategoriaUrl?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _unitOfWork.Categorias.AddAsync(categoria, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<CategoriaDto>(categoria);
    }

    public async Task<CategoriaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        return categoria != null ? _mapper.Map<CategoriaDto>(categoria) : null;
    }

    public async Task<IEnumerable<CategoriaDto>> GetAllAsync(bool? activo = null, CancellationToken ct = default)
    {
        IEnumerable<Categoria> categorias;

        if (activo.HasValue)
        {
            categorias = await _unitOfWork.Categorias.ListAsync(
                filter: c => c.Activo == activo.Value,
                ct: ct);
        }
        else
        {
            categorias = await _unitOfWork.Categorias.ListAsync(ct: ct);
        }

        return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoriaDto dto, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        // Validate name uniqueness (excluding current categoria)
        var existingCategoria = await _unitOfWork.Categorias.GetByNameAsync(dto.Nombre, ct);
        if (existingCategoria != null && existingCategoria.Id != id)
        {
            throw new BusinessRuleException($"Ya existe otra categoría con el nombre '{dto.Nombre}'.");
        }

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Descripcion = dto.Descripcion?.Trim();
        categoria.ImagenCategoriaUrl = dto.ImagenCategoriaUrl?.Trim();

        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        if (categoria.Activo)
        {
            throw new BusinessRuleException("La categoría ya está activa.");
        }

        categoria.Activo = true;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        if (!categoria.Activo)
        {
            throw new BusinessRuleException("La categoría ya está desactivada.");
        }

        // Check if categoria has products
        var hasProducts = await _unitOfWork.Categorias.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede desactivar la categoría porque tiene productos asignados. " +
                "Reasigne los productos a otra categoría antes de desactivarla.");
        }

        categoria.Activo = false;
        _unitOfWork.Categorias.Update(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeletePermanentlyAsync(Guid id, CancellationToken ct = default)
    {
        var categoria = await _unitOfWork.Categorias.GetByIdAsync(id, ct);
        if (categoria == null)
        {
            throw new NotFoundException("Categoria", id);
        }

        // Check if categoria has products
        var hasProducts = await _unitOfWork.Categorias.HasProductsAsync(id, ct);
        if (hasProducts)
        {
            throw new BusinessRuleException(
                "No se puede eliminar la categoría porque tiene productos asignados. " +
                "Elimine o reasigne todos los productos antes de eliminar la categoría.");
        }

        _unitOfWork.Categorias.Remove(categoria);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
