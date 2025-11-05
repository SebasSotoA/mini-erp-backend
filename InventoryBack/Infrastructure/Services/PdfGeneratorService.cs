using InventoryBack.Application.DTOs;
using InventoryBack.Infrastructure.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventoryBack.Infrastructure.Services;

/// <summary>
/// PDF generator service using QuestPDF.
/// </summary>
public class PdfGeneratorService : IPdfGenerator
{
    public Task<byte[]> GenerateInventarioResumenPdfAsync(InventarioResumenDto resumen)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // ========== HEADER ==========
                page.Header().Column(column =>
                {
                    column.Item().Text("Reporte de Inventario").FontSize(20).Bold();
                    column.Item().Text($"Fecha: {resumen.FechaGeneracion:dd/MM/yyyy HH:mm}").FontSize(10);

                    // Applied filters
                    if (resumen.FiltrosAplicados != null && resumen.FiltrosAplicados.Any())
                    {
                        column.Item().PaddingTop(10).Text("Filtros aplicados:").FontSize(11).Bold();
                        foreach (var filtro in resumen.FiltrosAplicados)
                        {
                            column.Item().Text($"• {filtro.Key}: {filtro.Value}").FontSize(9);
                        }
                    }

                    column.Item().PaddingTop(10).LineHorizontal(1);
                });

                // ========== CONTENT ==========
                page.Content().PaddingVertical(15).Column(column =>
                {
                    // Summary totals
                    column.Item().PaddingBottom(15).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Stock Total:").FontSize(12).Bold();
                            col.Item().Text($"{resumen.StockTotal:N0} unidades").FontSize(14);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Valor Total:").FontSize(12).Bold();
                            col.Item().Text($"${resumen.ValorTotal:N2}").FontSize(14);
                        });
                    });

                    // Products table
                    column.Item().Table(table =>
                    {
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Nombre
                            columns.RelativeColumn(2); // SKU
                            columns.RelativeColumn(2); // Bodega
                            columns.RelativeColumn(1); // Cantidad
                            columns.RelativeColumn(2); // Costo Unit.
                            columns.RelativeColumn(2); // Valor Total
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Producto");
                            header.Cell().Element(HeaderStyle).Text("SKU");
                            header.Cell().Element(HeaderStyle).Text("Bodega");
                            header.Cell().Element(HeaderStyle).Text("Cantidad");
                            header.Cell().Element(HeaderStyle).Text("Costo Unit.");
                            header.Cell().Element(HeaderStyle).Text("Valor Total");
                        });

                        // Data rows
                        foreach (var producto in resumen.Productos)
                        {
                            table.Cell().Element(CellStyle).Text(producto.Nombre);
                            table.Cell().Element(CellStyle).Text(producto.CodigoSku);
                            table.Cell().Element(CellStyle).Text(producto.Bodega);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{producto.Cantidad:N0}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"${producto.CostoUnitario:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"${producto.ValorTotal:N2}").Bold();
                        }
                    });
                });

                // ========== FOOTER ==========
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    // Helper methods for styling
    private static IContainer HeaderStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten1)
            .Background(Colors.Grey.Lighten3)
            .Padding(5);
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }
}
