using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class ReshapeSkillIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_skills_skill_icons_skill_icon_id",
                schema: "content",
                table: "skills");

            migrationBuilder.DropIndex(
                name: "ix_skills_skill_icon_id",
                schema: "content",
                table: "skills");

            migrationBuilder.DropIndex(
                name: "ix_skill_icons_name",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_skill_icons",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.DropColumn(
                name: "skill_icon_id",
                schema: "content",
                table: "skills");

            migrationBuilder.Sql("DELETE FROM content.skill_icons;");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.AddColumn<int>(
                name: "skill_id",
                schema: "content",
                table: "skill_icons",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<short>(
                name: "level",
                schema: "content",
                table: "skill_icons",
                type: "smallint",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_skill_icons",
                schema: "content",
                table: "skill_icons",
                columns: ["skill_id", "level"]);

            migrationBuilder.AddCheckConstraint(
                name: "ck_skill_icons_level",
                schema: "content",
                table: "skill_icons",
                sql: "level BETWEEN 0 AND 255");

            migrationBuilder.AddForeignKey(
                name: "FK_skill_icons_skills_skill_id",
                schema: "content",
                table: "skill_icons",
                column: "skill_id",
                principalSchema: "content",
                principalTable: "skills",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_skill_icons_skills_skill_id",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.DropCheckConstraint(
                name: "ck_skill_icons_level",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_skill_icons",
                schema: "content",
                table: "skill_icons");

            migrationBuilder.Sql("DELETE FROM content.skill_icons;");

            migrationBuilder.DropColumn(name: "skill_id", schema: "content", table: "skill_icons");
            migrationBuilder.DropColumn(name: "level", schema: "content", table: "skill_icons");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "content",
                table: "skill_icons",
                type: "integer",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "skill_icon_id",
                schema: "content",
                table: "skills",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_skill_icons",
                schema: "content",
                table: "skill_icons",
                column: "id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_skills_skill_icons_skill_icon_id",
                schema: "content",
                table: "skills",
                column: "skill_icon_id",
                principalSchema: "content",
                principalTable: "skill_icons",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
