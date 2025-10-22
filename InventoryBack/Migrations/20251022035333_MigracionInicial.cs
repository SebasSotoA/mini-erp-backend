using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryBack.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bodegas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bodegas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CamposExtra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TipoDato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EsRequerido = table.Column<bool>(type: "boolean", nullable: false),
                    ValorPorDefecto = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamposExtra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImagenCategoriaUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Identificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Identificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Correo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnidadMedida = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrecioBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ImpuestoPorcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CostoInicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodigoSku = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImagenProductoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturasCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroFactura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BodegaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasCompra_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturasCompra_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturasVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroFactura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BodegaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FormaPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlazoPago = table.Column<int>(type: "integer", nullable: true),
                    MedioPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasVenta_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturasVenta_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: true),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductoBodegas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodegaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadInicial = table.Column<int>(type: "integer", nullable: false),
                    CantidadMinima = table.Column<int>(type: "integer", nullable: true),
                    CantidadMaxima = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoBodegas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoBodegas_Bodegas_BodegaId",
                        column: x => x.BodegaId,
                        principalTable: "Bodegas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoBodegas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoCamposExtra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampoExtraId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoCamposExtra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoCamposExtra_CamposExtra_CampoExtraId",
                        column: x => x.CampoExtraId,
                        principalTable: "CamposExtra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoCamposExtra_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturasCompraDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaCompraId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric", nullable: true),
                    Impuesto = table.Column<decimal>(type: "numeric", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasCompraDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasCompraDetalle_FacturasCompra_FacturaCompraId",
                        column: x => x.FacturaCompraId,
                        principalTable: "FacturasCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturasCompraDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturasVentaDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaVentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric", nullable: true),
                    Impuesto = table.Column<decimal>(type: "numeric", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasVentaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturasVentaDetalle_FacturasVenta_FacturaVentaId",
                        column: x => x.FacturaVentaId,
                        principalTable: "FacturasVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturasVentaDetalle_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturasCompra_BodegaId",
                table: "FacturasCompra",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasCompra_NumeroFactura",
                table: "FacturasCompra",
                column: "NumeroFactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasCompra_ProveedorId",
                table: "FacturasCompra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasCompraDetalle_FacturaCompraId",
                table: "FacturasCompraDetalle",
                column: "FacturaCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasCompraDetalle_ProductoId",
                table: "FacturasCompraDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_BodegaId",
                table: "FacturasVenta",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_NumeroFactura",
                table: "FacturasVenta",
                column: "NumeroFactura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVenta_VendedorId",
                table: "FacturasVenta",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVentaDetalle_FacturaVentaId",
                table: "FacturasVentaDetalle",
                column: "FacturaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasVentaDetalle_ProductoId",
                table: "FacturasVentaDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_BodegaId",
                table: "MovimientosInventario",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductoId",
                table: "MovimientosInventario",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoBodegas_BodegaId",
                table: "ProductoBodegas",
                column: "BodegaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoBodegas_ProductoId",
                table: "ProductoBodegas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoCamposExtra_CampoExtraId",
                table: "ProductoCamposExtra",
                column: "CampoExtraId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoCamposExtra_ProductoId",
                table: "ProductoCamposExtra",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoSku",
                table: "Productos",
                column: "CodigoSku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturasCompraDetalle");

            migrationBuilder.DropTable(
                name: "FacturasVentaDetalle");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "ProductoBodegas");

            migrationBuilder.DropTable(
                name: "ProductoCamposExtra");

            migrationBuilder.DropTable(
                name: "FacturasCompra");

            migrationBuilder.DropTable(
                name: "FacturasVenta");

            migrationBuilder.DropTable(
                name: "CamposExtra");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Bodegas");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
