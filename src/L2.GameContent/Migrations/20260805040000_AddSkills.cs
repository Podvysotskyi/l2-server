using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skill_icons",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_icons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    levels = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    skill_icon_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                    table.CheckConstraint("ck_skills_levels", "levels BETWEEN 1 AND 255");
                    table.ForeignKey(
                        name: "FK_skills_skill_icons_skill_icon_id",
                        column: x => x.skill_icon_id,
                        principalSchema: "content",
                        principalTable: "skill_icons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_skill_icons_name",
                schema: "content",
                table: "skill_icons",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_skill_icon_id",
                schema: "content",
                table: "skills",
                column: "skill_icon_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "skills", schema: "content");
            migrationBuilder.DropTable(name: "skill_icons", schema: "content");
        }
    }
}
