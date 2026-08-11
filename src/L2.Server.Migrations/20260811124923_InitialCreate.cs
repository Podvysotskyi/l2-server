using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    normalized_username = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                schema: "public",
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
                    is_mage = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "account_credentials",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_login_history",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
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
                        principalSchema: "public",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "account_sessions",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_session_tickets",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "account_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_sessions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    selected_character_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_sessions_account_sessions_account_session_id",
                        column: x => x.account_session_id,
                        principalSchema: "public",
                        principalTable: "account_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_login_history_account_time",
                schema: "public",
                table: "account_login_history",
                columns: new[] { "account_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_account_active",
                schema: "public",
                table: "account_sessions",
                columns: new[] { "account_id", "expires_at" },
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_account_sessions_token_hash",
                schema: "public",
                table: "account_sessions",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_normalized_email",
                schema: "public",
                table: "accounts",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_normalized_username",
                schema: "public",
                table: "accounts",
                column: "normalized_username",
                unique: true);

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
                name: "ix_characters_deletion_deadline",
                schema: "public",
                table: "characters",
                column: "delete_after",
                filter: "delete_after IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_characters_normalized_name",
                schema: "public",
                table: "characters",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_account_session_id",
                schema: "public",
                table: "game_session_tickets",
                column: "account_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_pending_expiry",
                schema: "public",
                table: "game_session_tickets",
                column: "expires_at",
                filter: "consumed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_tickets_token_hash",
                schema: "public",
                table: "game_session_tickets",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_access_token_hash",
                schema: "public",
                table: "game_sessions",
                column: "access_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_account_session_id",
                schema: "public",
                table: "game_sessions",
                column: "account_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_active_expiry",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "revoked_at", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_credentials",
                schema: "public");

            migrationBuilder.DropTable(
                name: "account_login_history",
                schema: "public");

            migrationBuilder.DropTable(
                name: "characters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "game_session_tickets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "game_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "account_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "public");
        }
    }
}
