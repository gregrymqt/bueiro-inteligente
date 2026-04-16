using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations;

public partial class AddUserRolesManyToMany : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_roles",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "FK_user_roles_roles_role_id",
                    column: x => x.role_id,
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_user_roles_role_id",
            table: "user_roles",
            column: "role_id"
        );

        migrationBuilder.Sql(
            @"INSERT INTO user_roles (user_id, role_id)
              SELECT id, role_id
              FROM users;"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_users_roles_role_id",
            table: "users"
        );

        migrationBuilder.DropIndex(name: "IX_users_role_id", table: "users");

        migrationBuilder.DropColumn(name: "role_id", table: "users");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "role_id",
            table: "users",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.Sql(
            @"UPDATE users
              SET role_id = picked.role_id
              FROM (
                  SELECT DISTINCT ON (user_id) user_id, role_id
                  FROM user_roles
                  ORDER BY user_id, role_id
              ) AS picked
              WHERE users.id = picked.user_id;"
        );

        migrationBuilder.AlterColumn<Guid>(
            name: "role_id",
            table: "users",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_users_role_id",
            table: "users",
            column: "role_id"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_users_roles_role_id",
            table: "users",
            column: "role_id",
            principalTable: "roles",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict
        );

        migrationBuilder.DropTable(name: "user_roles");
    }
}