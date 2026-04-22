using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421190000_RepairMissingUsersAuthColumns")]
public partial class RepairMissingUsersAuthColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_url character varying(2048);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS google_id character varying(255);
            ALTER TABLE users ADD COLUMN IF NOT EXISTS email_confirmed boolean NOT NULL DEFAULT FALSE;
            """
        );

        migrationBuilder.Sql(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_users_google_id"
            ON users (google_id);
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP INDEX IF EXISTS "IX_users_google_id";
            ALTER TABLE users DROP COLUMN IF EXISTS avatar_url;
            ALTER TABLE users DROP COLUMN IF EXISTS google_id;
            ALTER TABLE users DROP COLUMN IF EXISTS email_confirmed;
            """
        );
    }
}
