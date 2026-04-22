using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260421193000_RepairMissingUserRolesRelation")]
public partial class RepairMissingUserRolesRelation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE TABLE IF NOT EXISTS user_roles (
                user_id uuid NOT NULL,
                role_id uuid NOT NULL,
                CONSTRAINT "PK_user_roles" PRIMARY KEY (user_id, role_id),
                CONSTRAINT "FK_user_roles_roles_role_id" FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
                CONSTRAINT "FK_user_roles_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
            );
            """
        );

        migrationBuilder.Sql(
            """
            CREATE INDEX IF NOT EXISTS "IX_user_roles_role_id"
            ON user_roles (role_id);
            """
        );

        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = current_schema()
                      AND table_name = 'users'
                      AND column_name = 'role_id'
                ) THEN
                    INSERT INTO user_roles (user_id, role_id)
                    SELECT id, role_id
                    FROM users
                    WHERE role_id IS NOT NULL
                    ON CONFLICT DO NOTHING;

                    ALTER TABLE users DROP CONSTRAINT IF EXISTS "FK_users_roles_role_id";
                    DROP INDEX IF EXISTS "IX_users_role_id";
                    ALTER TABLE users DROP COLUMN IF EXISTS role_id;
                END IF;
            END $$;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'user_roles'
                ) THEN
                    ALTER TABLE users ADD COLUMN IF NOT EXISTS role_id uuid;

                    UPDATE users
                    SET role_id = picked.role_id
                    FROM (
                        SELECT DISTINCT ON (user_id) user_id, role_id
                        FROM user_roles
                        ORDER BY user_id, role_id
                    ) AS picked
                    WHERE users.id = picked.user_id;

                    ALTER TABLE users ALTER COLUMN role_id SET NOT NULL;
                    CREATE INDEX IF NOT EXISTS "IX_users_role_id" ON users (role_id);
                    ALTER TABLE users
                        ADD CONSTRAINT "FK_users_roles_role_id"
                        FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE RESTRICT;
                END IF;
            END $$;
            """
        );

        migrationBuilder.Sql("DROP TABLE IF EXISTS user_roles;");
    }
}
