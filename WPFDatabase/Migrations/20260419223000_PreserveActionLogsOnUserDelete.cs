using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WPFDatabase.Data;

#nullable disable

namespace WPFDatabase.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260419223000_PreserveActionLogsOnUserDelete")]
public partial class PreserveActionLogsOnUserDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "ActionLogs_new" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ActionLogs" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NULL,
                "UserLoginSnapshot" TEXT NOT NULL,
                "ActionType" TEXT NOT NULL,
                "EntityType" TEXT NOT NULL,
                "EntityId" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "Details" TEXT NOT NULL,
                CONSTRAINT "FK_ActionLogs_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
            );
            """);

        migrationBuilder.Sql("""
            INSERT INTO "ActionLogs_new" (
                "Id",
                "UserId",
                "UserLoginSnapshot",
                "ActionType",
                "EntityType",
                "EntityId",
                "CreatedAt",
                "Details"
            )
            SELECT
                logs."Id",
                logs."UserId",
                COALESCE(users."Login", ''),
                logs."ActionType",
                logs."EntityType",
                logs."EntityId",
                logs."CreatedAt",
                logs."Details"
            FROM "ActionLogs" AS logs
            LEFT JOIN "Users" AS users ON users."Id" = logs."UserId";
            """);

        migrationBuilder.Sql("""DROP TABLE "ActionLogs";""");
        migrationBuilder.Sql("""ALTER TABLE "ActionLogs_new" RENAME TO "ActionLogs";""");
        migrationBuilder.Sql("""CREATE INDEX "IX_ActionLogs_UserId" ON "ActionLogs" ("UserId");""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE "ActionLogs_old" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ActionLogs" PRIMARY KEY AUTOINCREMENT,
                "UserId" INTEGER NOT NULL,
                "ActionType" TEXT NOT NULL,
                "EntityType" TEXT NOT NULL,
                "EntityId" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "Details" TEXT NOT NULL,
                CONSTRAINT "FK_ActionLogs_Users_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
            );
            """);

        migrationBuilder.Sql("""
            INSERT INTO "ActionLogs_old" (
                "Id",
                "UserId",
                "ActionType",
                "EntityType",
                "EntityId",
                "CreatedAt",
                "Details"
            )
            SELECT
                "Id",
                "UserId",
                "ActionType",
                "EntityType",
                "EntityId",
                "CreatedAt",
                "Details"
            FROM "ActionLogs"
            WHERE "UserId" IS NOT NULL;
            """);

        migrationBuilder.Sql("""DROP TABLE "ActionLogs";""");
        migrationBuilder.Sql("""ALTER TABLE "ActionLogs_old" RENAME TO "ActionLogs";""");
        migrationBuilder.Sql("""CREATE INDEX "IX_ActionLogs_UserId" ON "ActionLogs" ("UserId");""");
    }
}
