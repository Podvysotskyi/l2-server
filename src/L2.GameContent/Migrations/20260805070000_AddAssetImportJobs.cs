using System;
using L2.GameContent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations;

[DbContext(typeof(GameContentDbContext))]
[Migration("20260805070000_AddAssetImportJobs")]
public sealed class AddAssetImportJobs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "asset_import_jobs",
            schema: "content",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                source_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                source_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                total_count = table.Column<int>(type: "integer", nullable: false),
                processed_count = table.Column<int>(type: "integer", nullable: false),
                placeholder_count = table.Column<int>(type: "integer", nullable: false),
                warnings_json = table.Column<string>(type: "jsonb", nullable: false),
                error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_asset_import_jobs", item => item.id));

        migrationBuilder.CreateIndex(
            name: "ix_asset_import_jobs_claim",
            schema: "content",
            table: "asset_import_jobs",
            columns: new[] { "kind", "status", "requested_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "asset_import_jobs", schema: "content");
}
