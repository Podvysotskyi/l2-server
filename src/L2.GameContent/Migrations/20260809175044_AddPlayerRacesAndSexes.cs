using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerRacesAndSexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_classes",
                schema: "content");

            migrationBuilder.CreateTable(
                name: "player_races",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_races", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_sexes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_sexes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_classes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    parent_class_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_classes", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_classes_player_classes_parent_class_id_player_sex_id~",
                        columns: x => new { x.parent_class_id, x.player_sex_id, x.player_race_id },
                        principalSchema: "content",
                        principalTable: "player_classes",
                        principalColumns: new[] { "id", "player_sex_id", "player_race_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_classes_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "content",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_classes_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "content",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_name_sex_race",
                schema: "content",
                table: "player_classes",
                columns: new[] { "name", "player_sex_id", "player_race_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_parent_sex_race",
                schema: "content",
                table: "player_classes",
                columns: new[] { "parent_class_id", "player_sex_id", "player_race_id" });

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_player_race_id",
                schema: "content",
                table: "player_classes",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_player_sex_id",
                schema: "content",
                table: "player_classes",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_races_name",
                schema: "content",
                table: "player_races",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_sexes_name",
                schema: "content",
                table: "player_sexes",
                column: "name",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_classes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "player_races",
                schema: "content");

            migrationBuilder.DropTable(
                name: "player_sexes",
                schema: "content");

            migrationBuilder.CreateTable(
                name: "player_classes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    parent_class_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_classes", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_classes_player_classes_parent_class_id",
                        column: x => x.parent_class_id,
                        principalSchema: "content",
                        principalTable: "player_classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_name",
                schema: "content",
                table: "player_classes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_parent_class_id",
                schema: "content",
                table: "player_classes",
                column: "parent_class_id");

        }
    }
}
