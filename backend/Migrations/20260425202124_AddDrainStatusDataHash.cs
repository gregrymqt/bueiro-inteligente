using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDrainStatusDataHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "data_hash",
                table: "drain_status",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE drain_status SET data_hash = encode(sha256(cast(id_bueiro || '|' || distancia_cm::text || '|' || to_char(ultima_atualizacao at time zone 'UTC', 'YYYY-MM-DD\"T\"HH24:MI:SS.US0Z') as bytea)), 'hex') WHERE data_hash = '';");

            migrationBuilder.CreateIndex(
                name: "IX_drain_status_data_hash",
                table: "drain_status",
                column: "data_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drain_status_id_bueiro_ultima_atualizacao",
                table: "drain_status",
                columns: new[] { "id_bueiro", "ultima_atualizacao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_drain_status_data_hash",
                table: "drain_status");

            migrationBuilder.DropIndex(
                name: "IX_drain_status_id_bueiro_ultima_atualizacao",
                table: "drain_status");

            migrationBuilder.DropColumn(
                name: "data_hash",
                table: "drain_status");
        }
    }
}
