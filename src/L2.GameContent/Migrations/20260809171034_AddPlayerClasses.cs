using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_classes",
                schema: "content");
        }
    }
}
