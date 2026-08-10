using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.PlayerIdentity.Migrations
{
    /// <inheritdoc />
    public partial class InitialPlayerIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    normalized_username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "account_credentials",
                columns: table => new
                {
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_credentials", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_account_credentials_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_login_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    normalized_username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    failure_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_login_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_login_history_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "account_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_sessions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_session_tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_session_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_session_tickets_account_sessions_account_session_id",
                        column: x => x.account_session_id,
                        principalTable: "account_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_login_history_account_time",
                table: "account_login_history",
                columns: new[] { "account_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_account_active",
                table: "account_sessions",
                columns: new[] { "account_id", "expires_at" },
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_token_hash",
                table: "account_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_normalized_username",
                table: "accounts",
                column: "normalized_username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_account_session_id",
                table: "game_session_tickets",
                column: "account_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_pending_expiry",
                table: "game_session_tickets",
                column: "expires_at",
                filter: "consumed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_token_hash",
                table: "game_session_tickets",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_credentials");

            migrationBuilder.DropTable(
                name: "account_login_history");

            migrationBuilder.DropTable(
                name: "game_session_tickets");

            migrationBuilder.DropTable(
                name: "account_sessions");

            migrationBuilder.DropTable(
                name: "accounts");
        }
    }
}
