using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.GameContent.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNoneRaceAndMakeNpcRaceNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "npc_race_id",
                schema: "content",
                table: "npcs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql(
                "UPDATE content.npcs SET npc_race_id = NULL WHERE npc_race_id = 19; " +
                "DELETE FROM content.npc_races WHERE id = 19;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "INSERT INTO content.npc_races (id, name) VALUES (19, 'NONE'); " +
                "UPDATE content.npcs SET npc_race_id = 19 WHERE npc_race_id IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "npc_race_id",
                schema: "content",
                table: "npcs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
