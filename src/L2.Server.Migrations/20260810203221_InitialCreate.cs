using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "asset_catalogs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_folder = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    source_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    protocol = table.Column<int>(type: "integer", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_import_jobs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    source_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    skipped_count = table.Column<int>(type: "integer", nullable: false),
                    warnings_json = table.Column<string>(type: "jsonb", nullable: false),
                    error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_import_jobs", x => x.id);
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
                name: "npc_races",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_races", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "npc_sexes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_sexes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "npc_types",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npc_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_races",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_races", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_sexes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_sexes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill_operate_types",
                schema: "public",
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
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_target_types", x => x.id);
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
                name: "asset_catalog_groups",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalog_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_catalog_groups_asset_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "public",
                        principalTable: "asset_catalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asset_catalog_items",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    group_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_catalog_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_catalog_items_asset_catalogs_catalog_id",
                        column: x => x.catalog_id,
                        principalSchema: "public",
                        principalTable: "asset_catalogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "npcs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    npc_type_id = table.Column<int>(type: "integer", nullable: false),
                    npc_race_id = table.Column<int>(type: "integer", nullable: true),
                    npc_sex_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_npcs", x => x.id);
                    table.CheckConstraint("ck_npcs_level", "level BETWEEN 1 AND 255");
                    table.ForeignKey(
                        name: "FK_npcs_npc_races_npc_race_id",
                        column: x => x.npc_race_id,
                        principalSchema: "public",
                        principalTable: "npc_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_npcs_npc_sexes_npc_sex_id",
                        column: x => x.npc_sex_id,
                        principalSchema: "public",
                        principalTable: "npc_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_npcs_npc_types_npc_type_id",
                        column: x => x.npc_type_id,
                        principalSchema: "public",
                        principalTable: "npc_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_classes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_mage = table.Column<bool>(type: "boolean", nullable: false),
                    parent_class_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_classes", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_classes_player_classes_parent_class_id_player_sex_id~",
                        columns: x => new { x.parent_class_id, x.player_sex_id, x.player_race_id },
                        principalSchema: "public",
                        principalTable: "player_classes",
                        principalColumns: new[] { "id", "player_sex_id", "player_race_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_classes_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "public",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_classes_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "public",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_faces",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_faces", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_faces_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "public",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_faces_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "public",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_hair_colors",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_hair_colors", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_hair_colors_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "public",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_hair_colors_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "public",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "player_hair_styles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    player_sex_id = table.Column<int>(type: "integer", nullable: false),
                    player_race_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_hair_styles", x => new { x.id, x.player_sex_id, x.player_race_id });
                    table.ForeignKey(
                        name: "FK_player_hair_styles_player_races_player_race_id",
                        column: x => x.player_race_id,
                        principalSchema: "public",
                        principalTable: "player_races",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_player_hair_styles_player_sexes_player_sex_id",
                        column: x => x.player_sex_id,
                        principalSchema: "public",
                        principalTable: "player_sexes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    levels = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    skill_operate_type_id = table.Column<int>(type: "integer", nullable: true),
                    skill_target_type_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                    table.CheckConstraint("ck_skills_levels", "levels BETWEEN 1 AND 255");
                    table.ForeignKey(
                        name: "FK_skills_skill_operate_types_skill_operate_type_id",
                        column: x => x.skill_operate_type_id,
                        principalSchema: "public",
                        principalTable: "skill_operate_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_skills_skill_target_types_skill_target_type_id",
                        column: x => x.skill_target_type_id,
                        principalSchema: "public",
                        principalTable: "skill_target_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "skill_icons",
                schema: "public",
                columns: table => new
                {
                    skill_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_icons", x => new { x.skill_id, x.level });
                    table.CheckConstraint("ck_skill_icons_level", "level BETWEEN 1 AND 255");
                    table.ForeignKey(
                        name: "FK_skill_icons_skills_skill_id",
                        column: x => x.skill_id,
                        principalSchema: "public",
                        principalTable: "skills",
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
                name: "ix_asset_catalog_groups_catalog_name",
                schema: "public",
                table: "asset_catalog_groups",
                columns: new[] { "catalog_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_group_name",
                schema: "public",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "group_name", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_name",
                schema: "public",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalog_items_catalog_status",
                schema: "public",
                table: "asset_catalog_items",
                columns: new[] { "catalog_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_asset_catalogs_active_kind",
                schema: "public",
                table: "asset_catalogs",
                column: "kind",
                unique: true,
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_asset_import_jobs_active_kind",
                schema: "public",
                table: "asset_import_jobs",
                column: "kind",
                unique: true,
                filter: "\"status\" IN ('queued', 'running')");

            migrationBuilder.CreateIndex(
                name: "ix_asset_import_jobs_claim",
                schema: "public",
                table: "asset_import_jobs",
                columns: new[] { "kind", "status", "requested_at" });

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
                name: "ix_game_sessions_account_session_id",
                schema: "public",
                table: "game_sessions",
                column: "account_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_active_expiry",
                schema: "public",
                table: "game_sessions",
                columns: new[] { "revoked_at", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_npc_races_name",
                schema: "public",
                table: "npc_races",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npc_sexes_name",
                schema: "public",
                table: "npc_sexes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npc_types_name",
                schema: "public",
                table: "npc_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_race_id",
                schema: "public",
                table: "npcs",
                column: "npc_race_id");

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_sex_id",
                schema: "public",
                table: "npcs",
                column: "npc_sex_id");

            migrationBuilder.CreateIndex(
                name: "ix_npcs_npc_type_id",
                schema: "public",
                table: "npcs",
                column: "npc_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_name_sex_race",
                schema: "public",
                table: "player_classes",
                columns: new[] { "name", "player_sex_id", "player_race_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_parent_sex_race",
                schema: "public",
                table: "player_classes",
                columns: new[] { "parent_class_id", "player_sex_id", "player_race_id" });

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_player_race_id",
                schema: "public",
                table: "player_classes",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_classes_player_sex_id",
                schema: "public",
                table: "player_classes",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_faces_player_race_id",
                schema: "public",
                table: "player_faces",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_faces_player_sex_id",
                schema: "public",
                table: "player_faces",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_colors_player_race_id",
                schema: "public",
                table: "player_hair_colors",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_colors_player_sex_id",
                schema: "public",
                table: "player_hair_colors",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_styles_player_race_id",
                schema: "public",
                table: "player_hair_styles",
                column: "player_race_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_hair_styles_player_sex_id",
                schema: "public",
                table: "player_hair_styles",
                column: "player_sex_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_races_name",
                schema: "public",
                table: "player_races",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_sexes_name",
                schema: "public",
                table: "player_sexes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skill_operate_types_name",
                schema: "public",
                table: "skill_operate_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skill_target_types_name",
                schema: "public",
                table: "skill_target_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_skill_operate_type_id",
                schema: "public",
                table: "skills",
                column: "skill_operate_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_skills_skill_target_type_id",
                schema: "public",
                table: "skills",
                column: "skill_target_type_id");
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
                name: "asset_catalog_groups",
                schema: "public");

            migrationBuilder.DropTable(
                name: "asset_catalog_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "asset_import_jobs",
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
                name: "npcs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_classes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_faces",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_hair_colors",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_hair_styles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "skill_icons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "asset_catalogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "account_sessions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "npc_races",
                schema: "public");

            migrationBuilder.DropTable(
                name: "npc_sexes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "npc_types",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_races",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_sexes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "skills",
                schema: "public");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "skill_operate_types",
                schema: "public");

            migrationBuilder.DropTable(
                name: "skill_target_types",
                schema: "public");
        }
    }
}
