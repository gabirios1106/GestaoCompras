using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoCompras.API.Migrations
{
    /// <inheritdoc />
    public partial class ObservationOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observation",
                table: "Order",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observation",
                table: "Order");
        }
    }
}
