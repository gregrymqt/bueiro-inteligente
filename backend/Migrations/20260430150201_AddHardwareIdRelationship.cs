using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareIdRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_drains_hardware_id",
                table: "drains");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_drains_hardware_id",
                table: "drains",
                column: "hardware_id");

            migrationBuilder.AddForeignKey(
                name: "fk_drain_status_drains_hardware_id",
                table: "drain_status",
                column: "id_bueiro",
                principalTable: "drains",
                principalColumn: "hardware_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_drain_status_drains_hardware_id",
                table: "drain_status");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_drains_hardware_id",
                table: "drains");

            migrationBuilder.CreateIndex(
                name: "IX_drains_hardware_id",
                table: "drains",
                column: "hardware_id",
                unique: true);
        }
    }
}
