using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_catalogs",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_folder = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    source_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    protocol = table.Column<int>(type: "integer", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_catalog_groups",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalog_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_catalog_groups_asset_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "content",
                        principalTable: "asset_catalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asset_catalog_items",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    group_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalog_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_catalog_items_asset_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "content",
                        principalTable: "asset_catalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_groups_catalog_name",
                schema: "content",
                table: "asset_catalog_groups",
                columns: new[] { "catalog_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_group_name",
                schema: "content",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "group_name", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_name",
                schema: "content",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_status",
                schema: "content",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalogs_active_kind",
                schema: "content",
                table: "asset_catalogs",
                column: "kind",
                unique: true,
                filter: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_catalog_groups",
                schema: "content");

            migrationBuilder.DropTable(
                name: "asset_catalog_items",
                schema: "content");

            migrationBuilder.DropTable(
                name: "asset_catalogs",
                schema: "content");
        }
    }
}
