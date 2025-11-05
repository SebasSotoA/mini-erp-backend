using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryBack.Migrations
{
    /// <inheritdoc />
    public partial class AddBodegaPrincipalIdToProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BodegaPrincipalId",
                table: "Productos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodegaPrincipalId",
                table: "Productos");
        }
    }
}
