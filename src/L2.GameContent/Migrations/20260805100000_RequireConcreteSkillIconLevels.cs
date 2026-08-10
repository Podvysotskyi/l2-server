using L2.GameContent;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations;

[DbContext(typeof(GameContentDbContext))]
[Migration("20260805100000_RequireConcreteSkillIconLevels")]
public sealed class RequireConcreteSkillIconLevels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_skill_icons_level",
            schema: "content",
            table: "skill_icons");

        migrationBuilder.Sql("DELETE FROM content.skill_icons WHERE level = 0;");

        migrationBuilder.AddCheckConstraint(
            name: "ck_skill_icons_level",
            schema: "content",
            table: "skill_icons",
            sql: "level BETWEEN 1 AND 255");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_skill_icons_level",
            schema: "content",
            table: "skill_icons");

        migrationBuilder.AddCheckConstraint(
            name: "ck_skill_icons_level",
            schema: "content",
            table: "skill_icons",
            sql: "level BETWEEN 0 AND 255");
    }
}
