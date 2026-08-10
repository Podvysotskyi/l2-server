using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class InitialGameContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "npc_races",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_races", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "npc_sexes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_sexes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "npc_types",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "npcs",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    npc_type_id = table.Column<int>(type: "integer", nullable: false),
                    npc_race_id = table.Column<int>(type: "integer", nullable: false),
                    npc_sex_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npcs", x => x.id);
                    table.CheckConstraint("ck_npcs_level", "level BETWEEN 1 AND 255");
                    table.ForeignKey(
                        name: "FK_npcs_npc_races_npc_race_id",
                        column: x => x.npc_race_id,
                        principalSchema: "content",
                        principalTable: "npc_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_npcs_npc_sexes_npc_sex_id",
                        column: x => x.npc_sex_id,
                        principalSchema: "content",
                        principalTable: "npc_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_npcs_npc_types_npc_type_id",
                        column: x => x.npc_type_id,
                        principalSchema: "content",
                        principalTable: "npc_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_npc_races_name",
                schema: "content",
                table: "npc_races",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npc_sexes_name",
                schema: "content",
                table: "npc_sexes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npc_types_name",
                schema: "content",
                table: "npc_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_race_id",
                schema: "content",
                table: "npcs",
                column: "npc_race_id");

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_sex_id",
                schema: "content",
                table: "npcs",
                column: "npc_sex_id");

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_type_id",
                schema: "content",
                table: "npcs",
                column: "npc_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "npcs",
                schema: "content");

            migrationBuilder.DropTable(
                name: "npc_races",
                schema: "content");

            migrationBuilder.DropTable(
                name: "npc_sexes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "npc_types",
                schema: "content");
        }
    }
}
