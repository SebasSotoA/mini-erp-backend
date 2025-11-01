using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryBack.Migrations
{
    /// <inheritdoc />
    public partial class Add_Descripcion_To_CampoExtra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "CamposExtra",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "CamposExtra");
        }
    }
}
