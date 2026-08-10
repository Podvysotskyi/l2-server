using L2.GameContent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations;

[DbContext(typeof(GameContentDbContext))]
[Migration("20260805080000_EnforceSingleActiveAssetImport")]
public sealed class EnforceSingleActiveAssetImport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.CreateIndex(
            name: "ix_asset_import_jobs_active_kind",
            schema: "content",
            table: "asset_import_jobs",
            column: "kind",
            unique: true,
            filter: "\"status\" IN ('queued', 'running')");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropIndex(
            name: "ix_asset_import_jobs_active_kind",
            schema: "content",
            table: "asset_import_jobs");
}
