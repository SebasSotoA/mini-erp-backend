using InventoryBack.Application.DTOs;

namespace InventoryBack.Infrastructure.Interfaces;

/// <summary>
/// Interface for PDF generation service.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates a PDF report from inventory summary data.
    /// </summary>
    /// <param name="resumen">Inventory summary data</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateInventarioResumenPdfAsync(InventarioResumenDto resumen);
}
