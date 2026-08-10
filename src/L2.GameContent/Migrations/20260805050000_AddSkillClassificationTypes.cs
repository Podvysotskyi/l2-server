using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillClassificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skill_operate_types",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_operate_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_target_types",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_target_types", x => x.id);
                });

            migrationBuilder.AddColumn<int>(
                name: "skill_operate_type_id",
                schema: "content",
                table: "skills",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "skill_target_type_id",
                schema: "content",
                table: "skills",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_skill_operate_types_name",
                schema: "content",
                table: "skill_operate_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skill_target_types_name",
                schema: "content",
                table: "skill_target_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_skill_operate_type_id",
                schema: "content",
                table: "skills",
                column: "skill_operate_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_skills_skill_target_type_id",
                schema: "content",
                table: "skills",
                column: "skill_target_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_skills_skill_operate_types_skill_operate_type_id",
                schema: "content",
                table: "skills",
                column: "skill_operate_type_id",
                principalSchema: "content",
                principalTable: "skill_operate_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_skills_skill_target_types_skill_target_type_id",
                schema: "content",
                table: "skills",
                column: "skill_target_type_id",
                principalSchema: "content",
                principalTable: "skill_target_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_skills_skill_operate_types_skill_operate_type_id",
                schema: "content",
                table: "skills");

            migrationBuilder.DropForeignKey(
                name: "FK_skills_skill_target_types_skill_target_type_id",
                schema: "content",
                table: "skills");

            migrationBuilder.DropColumn(name: "skill_operate_type_id", schema: "content", table: "skills");
            migrationBuilder.DropColumn(name: "skill_target_type_id", schema: "content", table: "skills");
            migrationBuilder.DropTable(name: "skill_operate_types", schema: "content");
            migrationBuilder.DropTable(name: "skill_target_types", schema: "content");
        }
    }
}
