using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryBack.Migrations
{
    /// <inheritdoc />
    public partial class Renombrarcolumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductoBodegas_ProductoId",
                table: "ProductoBodegas");

            migrationBuilder.RenameColumn(
                name: "CantidadInicial",
                table: "ProductoBodegas",
                newName: "StockActual");

            migrationBuilder.RenameColumn(
                name: "FacturaId",
                table: "MovimientosInventario",
                newName: "FacturaVentaId");

            migrationBuilder.AddColumn<Guid>(
                name: "FacturaCompraId",
                table: "MovimientosInventario",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoBodega_Producto_Bodega",
                table: "ProductoBodegas",
                columns: new[] { "ProductoId", "BodegaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FacturaCompraId",
                table: "MovimientosInventario",
                column: "FacturaCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FacturaVentaId",
                table: "MovimientosInventario",
                column: "FacturaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Fecha",
                table: "MovimientosInventario",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                table: "MovimientosInventario",
                column: "TipoMovimiento");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_FacturasCompra_FacturaCompraId",
                table: "MovimientosInventario",
                column: "FacturaCompraId",
                principalTable: "FacturasCompra",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_FacturasVenta_FacturaVentaId",
                table: "MovimientosInventario",
                column: "FacturaVentaId",
                principalTable: "FacturasVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_FacturasCompra_FacturaCompraId",
                table: "MovimientosInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_FacturasVenta_FacturaVentaId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_ProductoBodega_Producto_Bodega",
                table: "ProductoBodegas");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_FacturaCompraId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_FacturaVentaId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_Fecha",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "FacturaCompraId",
                table: "MovimientosInventario");

            migrationBuilder.RenameColumn(
                name: "StockActual",
                table: "ProductoBodegas",
                newName: "CantidadInicial");

            migrationBuilder.RenameColumn(
                name: "FacturaVentaId",
                table: "MovimientosInventario",
                newName: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoBodegas_ProductoId",
                table: "ProductoBodegas",
                column: "ProductoId");
        }
    }
}
