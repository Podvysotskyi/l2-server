using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGameServersAndGlobalLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_account_login_history_game_versions_game_version",
                schema: "public",
                table: "account_login_history");

            migrationBuilder.DropForeignKey(
                name: "FK_account_sessions_game_versions_game_version",
                schema: "public",
                table: "account_sessions");

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
                table: "account_sessions");

            migrationBuilder.DropColumn(
                name: "game_version",
                schema: "public",
                table: "account_login_history");

            migrationBuilder.AddColumn<string>(
                name: "game_server",
                schema: "public",
                table: "game_sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "game_server",
                schema: "public",
                table: "game_session_tickets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "game_server",
                schema: "public",
                table: "characters",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_created",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "game_server", "account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_characters_account_slot",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "game_server", "account_id", "account_slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters",
                columns: new[] { "game_version", "game_server", "normalized_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "game_server",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "game_server",
                schema: "public",
                table: "game_session_tickets");

            migrationBuilder.DropColumn(
                name: "game_server",
                schema: "public",
                table: "characters");

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
        }
    }
}
