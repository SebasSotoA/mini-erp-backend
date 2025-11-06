using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Application.Services;

/// <summary>
/// Service for inventory summary and reporting.
/// </summary>
public class InventarioService : IInventarioService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventarioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<InventarioResumenDto> GetResumenAsync(
        InventarioFilterDto filters, 
        CancellationToken ct = default)
    {
        // ========== VALIDATE AND NORMALIZE PAGINATION ==========
        if (filters.Page < 1) filters.Page = 1;
        if (filters.PageSize < 1 || filters.PageSize > 1000) filters.PageSize = 50;

        // ========== BUILD QUERY WITH FILTERS ==========
        
        // Get all ProductoBodegas with related entities
        var productoBodegasQuery = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);
        
        // Apply filters
        var filteredData = productoBodegasQuery.AsQueryable();

        // Filter by multiple warehouses (if provided)
        if (filters.BodegaIds != null && filters.BodegaIds.Any())
        {
            filteredData = filteredData.Where(pb => filters.BodegaIds.Contains(pb.BodegaId));
        }

        // Get products with their bodegas
        var productos = new List<InventarioProductoDto>();
        decimal valorTotal = 0;
        int stockTotal = 0;
        
        foreach (var pb in filteredData)
        {
            var producto = await _unitOfWork.Products.GetByIdAsync(pb.ProductoId, ct);
            if (producto == null) continue;

            // Filter by status
            var estadoLower = filters.Estado?.ToLower() ?? "activo";
            if (estadoLower == "activo" && !producto.Activo) continue;
            if (estadoLower == "inactivo" && producto.Activo) continue;
            // "todos" includes all

            // Filter by multiple categories (if provided)
            if (filters.CategoriaIds != null && filters.CategoriaIds.Any())
            {
                // Skip if product has no category OR category not in the filter list
                if (!producto.CategoriaId.HasValue || !filters.CategoriaIds.Contains(producto.CategoriaId.Value))
                {
                    continue;
                }
            }

            // Filter by search query (name or SKU)
            if (!string.IsNullOrWhiteSpace(filters.Q))
            {
                var searchTerm = filters.Q.ToLower();
                var matchesName = producto.Nombre.ToLower().Contains(searchTerm);
                var matchesSku = !string.IsNullOrEmpty(producto.CodigoSku) && 
                                 producto.CodigoSku.ToLower().Contains(searchTerm);
                
                if (!matchesName && !matchesSku)
                {
                    continue;
                }
            }

            // Get bodega name
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(pb.BodegaId, ct);
            var bodegaNombre = bodega?.Nombre ?? "Sin Bodega";

            // Get category name (optional)
            string? categoriaNombre = null;
            if (producto.CategoriaId.HasValue)
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(producto.CategoriaId.Value, ct);
                categoriaNombre = categoria?.Nombre;
            }

            // Calculate values
            var cantidad = pb.StockActual;
            var costoUnitario = producto.CostoInicial;
            var valorProducto = cantidad * costoUnitario;

            productos.Add(new InventarioProductoDto
            {
                Nombre = producto.Nombre,
                CodigoSku = producto.CodigoSku ?? "N/A",
                Bodega = bodegaNombre,
                Cantidad = cantidad,
                CostoUnitario = costoUnitario,
                ValorTotal = valorProducto,
                Categoria = categoriaNombre
            });

            // Accumulate totals
            valorTotal += valorProducto;
            stockTotal += cantidad;
        }

        // ========== APPLY PAGINATION ==========
        
        var totalCount = productos.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);
        
        // Apply pagination to the list
        var paginatedProductos = productos
            .OrderBy(p => p.Nombre)
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToList();

        // ========== BUILD RESPONSE ==========
        
        var resumen = new InventarioResumenDto
        {
            ValorTotal = valorTotal,
            StockTotal = stockTotal,
            Productos = paginatedProductos,
            FechaGeneracion = DateTime.UtcNow,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        // Add applied filters for PDF header
        var filtrosAplicados = new Dictionary<string, string>();
        
        // Multiple warehouses
        if (filters.BodegaIds != null && filters.BodegaIds.Any())
        {
            var bodegaNames = new List<string>();
            foreach (var bodegaId in filters.BodegaIds)
            {
                var bodega = await _unitOfWork.Bodegas.GetByIdAsync(bodegaId, ct);
                if (bodega != null)
                {
                    bodegaNames.Add(bodega.Nombre);
                }
            }
            if (bodegaNames.Any())
            {
                filtrosAplicados["Bodegas"] = string.Join(", ", bodegaNames);
            }
        }
        
        // Multiple categories
        if (filters.CategoriaIds != null && filters.CategoriaIds.Any())
        {
            var categoriaNames = new List<string>();
            foreach (var categoriaId in filters.CategoriaIds)
            {
                var categoria = await _unitOfWork.Categorias.GetByIdAsync(categoriaId, ct);
                if (categoria != null)
                {
                    categoriaNames.Add(categoria.Nombre);
                }
            }
            if (categoriaNames.Any())
            {
                filtrosAplicados["Categorías"] = string.Join(", ", categoriaNames);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Estado))
        {
            filtrosAplicados["Estado"] = filters.Estado;
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Q))
        {
            filtrosAplicados["Búsqueda"] = filters.Q;
        }

        resumen.FiltrosAplicados = filtrosAplicados.Any() ? filtrosAplicados : null;

        return resumen;
    }
}
