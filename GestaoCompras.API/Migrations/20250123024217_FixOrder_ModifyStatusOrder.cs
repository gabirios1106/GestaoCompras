using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoCompras.API.Migrations
{
    /// <inheritdoc />
    public partial class FixOrder_ModifyStatusOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "Order",
                newName: "StatusOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusOrder",
                table: "Order",
                newName: "status");
        }
    }
}
