using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "google_id",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "email_confirmed",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.CreateIndex(
                name: "IX_users_google_id",
                table: "users",
                column: "google_id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_users_google_id", table: "users");

            migrationBuilder.DropColumn(name: "avatar_url", table: "users");

            migrationBuilder.DropColumn(name: "email_confirmed", table: "users");

            migrationBuilder.DropColumn(name: "google_id", table: "users");
        }
    }
}
