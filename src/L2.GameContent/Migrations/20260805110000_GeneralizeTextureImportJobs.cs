using L2.GameContent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations;

[DbContext(typeof(GameContentDbContext))]
[Migration("20260805110000_GeneralizeTextureImportJobs")]
public sealed class GeneralizeTextureImportJobs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.RenameColumn(
            name: "placeholder_count",
            schema: "content",
            table: "asset_import_jobs",
            newName: "skipped_count");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.RenameColumn(
            name: "skipped_count",
            schema: "content",
            table: "asset_import_jobs",
            newName: "placeholder_count");
}
