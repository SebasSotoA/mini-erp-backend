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
        // ========== BUILD QUERY WITH FILTERS ==========
        
        // Get all ProductoBodegas with related entities
        var productoBodegasQuery = await _unitOfWork.ProductoBodegas.ListAsync(ct: ct);
        
        // Apply filters
        var filteredData = productoBodegasQuery.AsQueryable();

        // Filter by warehouse
        if (filters.BodegaId.HasValue)
        {
            filteredData = filteredData.Where(pb => pb.BodegaId == filters.BodegaId.Value);
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

            // Filter by category
            if (filters.CategoriaId.HasValue && producto.CategoriaId != filters.CategoriaId.Value)
            {
                continue;
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

        // ========== BUILD RESPONSE ==========
        
        var resumen = new InventarioResumenDto
        {
            ValorTotal = valorTotal,
            StockTotal = stockTotal,
            Productos = productos.OrderBy(p => p.Nombre).ToList(),
            FechaGeneracion = DateTime.UtcNow
        };

        // Add applied filters for PDF header
        var filtrosAplicados = new Dictionary<string, string>();
        
        if (filters.BodegaId.HasValue)
        {
            var bodega = await _unitOfWork.Bodegas.GetByIdAsync(filters.BodegaId.Value, ct);
            if (bodega != null)
            {
                filtrosAplicados["Bodega"] = bodega.Nombre;
            }
        }
        
        if (filters.CategoriaId.HasValue)
        {
            var categoria = await _unitOfWork.Categorias.GetByIdAsync(filters.CategoriaId.Value, ct);
            if (categoria != null)
            {
                filtrosAplicados["Categoría"] = categoria.Nombre;
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
