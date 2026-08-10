using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.PlayerCharacters.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlayerCharacters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "player");

            migrationBuilder.CreateTable(
                name: "characters",
                schema: "player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_slot = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    base_class_id = table.Column<int>(type: "integer", nullable: false),
                    active_class_id = table.Column<int>(type: "integer", nullable: false),
                    face_id = table.Column<int>(type: "integer", nullable: false),
                    hair_style_id = table.Column<int>(type: "integer", nullable: false),
                    hair_color_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    experience = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    delete_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => x.id);
                    table.CheckConstraint("ck_characters_account_slot", "account_slot >= 0");
                    table.CheckConstraint("ck_characters_experience", "experience >= 0");
                    table.CheckConstraint("ck_characters_level", "level BETWEEN 1 AND 255");
                });

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_created",
                schema: "player",
                table: "characters",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_slot",
                schema: "player",
                table: "characters",
                columns: new[] { "account_id", "account_slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_deletion_deadline",
                schema: "player",
                table: "characters",
                column: "delete_after",
                filter: "delete_after IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_characters_normalized_name",
                schema: "player",
                table: "characters",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "characters",
                schema: "player");
        }
    }
}
