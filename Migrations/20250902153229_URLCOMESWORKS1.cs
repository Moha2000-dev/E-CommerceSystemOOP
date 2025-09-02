using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_CommerceSystem.Migrations
{
    /// <inheritdoc />
    public partial class URLCOMESWORKS1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Products_PID",
                table: "OrderProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Products_PID",
                table: "OrderProducts",
                column: "PID",
                principalTable: "Products",
                principalColumn: "PID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderProducts_Products_PID",
                table: "OrderProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProducts_Products_PID",
                table: "OrderProducts",
                column: "PID",
                principalTable: "Products",
                principalColumn: "PID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
