using InventoryBack.Application.DTOs;
using InventoryBack.Application.Interfaces;
using InventoryBack.Infrastructure.Interfaces;
using InventoryBack.API.Extensions;
using InventoryBack.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryBack.API.Controllers;

/// <summary>
/// Controller for inventory summary and reporting.
/// </summary>
[ApiController]
[Route("api/inventario")]
[Produces("application/json")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;
    private readonly IPdfGenerator _pdfGenerator;

    public InventarioController(
        IInventarioService inventarioService,
        IPdfGenerator pdfGenerator)
    {
        _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));
        _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
    }

    /// <summary>
    /// Gets an inventory summary with optional filters.
    /// </summary>
    /// <param name="filters">Filter parameters (bodegaId, categoriaId, estado, q)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inventory summary with total value and stock</returns>
    /// <example>
    /// GET /api/inventario/resumen?bodegaId=guid&estado=activo&q=laptop
    /// </example>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ApiResponse<InventarioResumenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InventarioResumenDto>>> GetResumen(
        [FromQuery] InventarioFilterDto filters,
        CancellationToken ct = default)
    {
        var resumen = await _inventarioService.GetResumenAsync(filters, ct);
        return this.OkResponse(resumen, "Resumen de inventario obtenido correctamente.");
    }

    /// <summary>
    /// Exports an inventory summary as a PDF file.
    /// </summary>
    /// <param name="filters">Filter parameters (same as /resumen)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>PDF file for download</returns>
    /// <example>
    /// GET /api/inventario/resumen/pdf?bodegaId=guid&estado=activo
    /// </example>
    [HttpGet("resumen/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumenPdf(
        [FromQuery] InventarioFilterDto filters,
        CancellationToken ct = default)
    {
        // Get inventory summary
        var resumen = await _inventarioService.GetResumenAsync(filters, ct);

        // Generate PDF
        var pdfBytes = await _pdfGenerator.GenerateInventarioResumenPdfAsync(resumen);

        // Generate filename with timestamp
        var fileName = $"inventario_resumen_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        // Return PDF file
        return File(pdfBytes, "application/pdf", fileName);
    }
}
