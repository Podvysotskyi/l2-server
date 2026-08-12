using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace L2.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGameVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_characters_account_created",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_account_slot",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters");

            migrationBuilder.AddColumn<string>(
                name: "game_version",
                schema: "public",
                table: "game_sessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "interlude");

            migrationBuilder.AddColumn<string>(
                name: "game_version",
                schema: "public",
                table: "game_session_tickets",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "interlude");

            migrationBuilder.AddColumn<string>(
                name: "game_version",
                schema: "public",
                table: "characters",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "interlude");

            migrationBuilder.AddColumn<string>(
                name: "game_version",
                schema: "public",
                table: "account_sessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "interlude");

            migrationBuilder.AddColumn<string>(
                name: "game_version",
                schema: "public",
                table: "account_login_history",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "interlude");

            migrationBuilder.CreateTable(
                name: "game_versions",
                schema: "public",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    display_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_versions", x => x.key);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "game_versions",
                columns: new[] { "key", "display_name", "sort_order" },
                values: new object[,]
                {
                    { "c1", "Chronicle 1", 10 },
                    { "c4", "Chronicle 4", 20 },
                    { "interlude", "Interlude", 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_sessions_game_version",
                schema: "public",
                table: "game_sessions",
                column: "game_version");

            migrationBuilder.CreateIndex(
                name: "IX_game_session_tickets_game_version",
                schema: "public",
                table: "game_session_tickets",
                column: "game_version");

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_created",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_slot",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "account_id", "account_slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_account_sessions_game_version",
                schema: "public",
                table: "account_sessions",
                column: "game_version");

            migrationBuilder.CreateIndex(
                name: "IX_account_login_history_game_version",
                schema: "public",
                table: "account_login_history",
                column: "game_version");

            migrationBuilder.CreateIndex(
                name: "ix_game_versions_display_name",
                schema: "public",
                table: "game_versions",
                column: "display_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_account_login_history_game_versions_game_version",
                schema: "public",
                table: "account_login_history",
                column: "game_version",
                principalSchema: "public",
                principalTable: "game_versions",
                principalColumn: "key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_account_sessions_game_versions_game_version",
                schema: "public",
                table: "account_sessions",
                column: "game_version",
                principalSchema: "public",
                principalTable: "game_versions",
                principalColumn: "key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_characters_game_versions_game_version",
                schema: "public",
                table: "characters",
                column: "game_version",
                principalSchema: "public",
                principalTable: "game_versions",
                principalColumn: "key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_game_session_tickets_game_versions_game_version",
                schema: "public",
                table: "game_session_tickets",
                column: "game_version",
                principalSchema: "public",
                principalTable: "game_versions",
                principalColumn: "key",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_game_sessions_game_versions_game_version",
                schema: "public",
                table: "game_sessions",
                column: "game_version",
                principalSchema: "public",
                principalTable: "game_versions",
                principalColumn: "key",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_login_history_game_versions_game_version",
                schema: "public",
                table: "account_login_history");

            migrationBuilder.DropForeignKey(
                name: "FK_account_sessions_game_versions_game_version",
                schema: "public",
                table: "account_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_characters_game_versions_game_version",
                schema: "public",
                table: "characters");

            migrationBuilder.DropForeignKey(
                name: "FK_game_session_tickets_game_versions_game_version",
                schema: "public",
                table: "game_session_tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_game_sessions_game_versions_game_version",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropTable(
                name: "game_versions",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_game_sessions_game_version",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropIndex(
                name: "IX_game_session_tickets_game_version",
                schema: "public",
                table: "game_session_tickets");

            migrationBuilder.DropIndex(
                name: "ix_characters_account_created",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_account_slot",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters");

            migrationBuilder.DropIndex(
                name: "IX_account_sessions_game_version",
                schema: "public",
                table: "account_sessions");

            migrationBuilder.DropIndex(
                name: "IX_account_login_history_game_version",
                schema: "public",
                table: "account_login_history");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "game_session_tickets");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "account_sessions");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "account_login_history");

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_created",
                schema: "public",
                table: "characters",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_slot",
                schema: "public",
                table: "characters",
                columns: new[] { "account_id", "account_slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters",
                column: "normalized_name",
                unique: true);
        }
    }
}
