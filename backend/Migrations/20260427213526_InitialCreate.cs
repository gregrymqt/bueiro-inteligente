using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drain_status",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_bueiro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    distancia_cm = table.Column<double>(type: "double precision", nullable: false),
                    nivel_obstrucao = table.Column<double>(type: "double precision", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    ultima_atualizacao = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    sincronizado_rows = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    data_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drain_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "home_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    icon_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_stats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "uploads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    extension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uploads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    google_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    hashed_password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "home_carousels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    upload_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    section = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_home_carousels", x => x.id);
                    table.ForeignKey(
                        name: "FK_home_carousels_uploads_upload_id",
                        column: x => x.upload_id,
                        principalTable: "uploads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "drains",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    hardware_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    max_height = table.Column<double>(type: "double precision", nullable: false, defaultValue: 120.0),
                    critical_threshold = table.Column<double>(type: "double precision", nullable: false, defaultValue: 80.0),
                    alert_threshold = table.Column<double>(type: "double precision", nullable: false, defaultValue: 50.0),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drains", x => x.id);
                    table.ForeignKey(
                        name: "FK_drains_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "uploads",
                columns: new[] { "id", "checksum", "content_type", "created_at", "extension", "file_name", "size", "storage_path" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "A1B2C3D4E5F6", "image/jpeg", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), ".jpg", "sample_home_photo.jpg", 102400L, "/var/www/uploads/11111111-1111-1111-1111-111111111111.jpg" });

            migrationBuilder.CreateIndex(
                name: "IX_drain_status_data_hash",
                table: "drain_status",
                column: "data_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drain_status_id_bueiro",
                table: "drain_status",
                column: "id_bueiro");

            migrationBuilder.CreateIndex(
                name: "IX_drain_status_id_bueiro_ultima_atualizacao",
                table: "drain_status",
                columns: new[] { "id_bueiro", "ultima_atualizacao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drains_hardware_id",
                table: "drains",
                column: "hardware_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_drains_UserId",
                table: "drains",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_home_carousels_upload_id",
                table: "home_carousels",
                column: "upload_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_google_id",
                table: "users",
                column: "google_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drain_status");

            migrationBuilder.DropTable(
                name: "drains");

            migrationBuilder.DropTable(
                name: "home_carousels");

            migrationBuilder.DropTable(
                name: "home_stats");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "uploads");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
