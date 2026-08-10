using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "access_token_hash",
                schema: "public",
                table: "game_sessions",
                type: "bytea",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE public.game_sessions
                SET access_token_hash = decode(md5(id::text), 'hex')
                WHERE access_token_hash IS NULL;
                """);

            migrationBuilder.AlterColumn<byte[]>(
                name: "access_token_hash",
                schema: "public",
                table: "game_sessions",
                type: "bytea",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_access_token_hash",
                schema: "public",
                table: "game_sessions",
                column: "access_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_game_sessions_access_token_hash",
                schema: "public",
                table: "game_sessions");

            migrationBuilder.DropColumn(
                name: "access_token_hash",
                schema: "public",
                table: "game_sessions");
        }
    }
}
