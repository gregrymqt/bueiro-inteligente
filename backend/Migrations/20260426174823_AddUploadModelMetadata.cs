using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadModelMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Checksum",
                table: "Uploads",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Uploads",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "alert_threshold",
                table: "drains",
                type: "double precision",
                nullable: false,
                defaultValue: 50.0);

            migrationBuilder.AddColumn<double>(
                name: "critical_threshold",
                table: "drains",
                type: "double precision",
                nullable: false,
                defaultValue: 80.0);

            migrationBuilder.AddColumn<double>(
                name: "max_height",
                table: "drains",
                type: "double precision",
                nullable: false,
                defaultValue: 120.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Checksum",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "alert_threshold",
                table: "drains");

            migrationBuilder.DropColumn(
                name: "critical_threshold",
                table: "drains");

            migrationBuilder.DropColumn(
                name: "max_height",
                table: "drains");
        }
    }
}
