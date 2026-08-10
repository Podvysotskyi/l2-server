using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerMageAndAppearances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_mage",
                schema: "content",
                table: "player_classes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "player_faces",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_faces", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_faces_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "content",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_faces_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "content",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_hair_colors",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_hair_colors", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_hair_colors_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "content",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_hair_colors_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "content",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_hair_styles",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_hair_styles", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_hair_styles_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "content",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_hair_styles_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "content",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_player_faces_player_race_id",
                schema: "content",
                table: "player_faces",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_faces_player_sex_id",
                schema: "content",
                table: "player_faces",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_colors_player_race_id",
                schema: "content",
                table: "player_hair_colors",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_colors_player_sex_id",
                schema: "content",
                table: "player_hair_colors",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_styles_player_race_id",
                schema: "content",
                table: "player_hair_styles",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_styles_player_sex_id",
                schema: "content",
                table: "player_hair_styles",
                column: "player_sex_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_faces",
                schema: "content");

            migrationBuilder.DropTable(
                name: "player_hair_colors",
                schema: "content");

            migrationBuilder.DropTable(
                name: "player_hair_styles",
                schema: "content");

            migrationBuilder.DropColumn(
                name: "is_mage",
                schema: "content",
                table: "player_classes");
        }
    }
}
