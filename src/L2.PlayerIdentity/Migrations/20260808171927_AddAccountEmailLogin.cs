using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace L2.PlayerIdentity.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountEmailLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "normalized_username",
                table: "account_login_history",
                newName: "normalized_email");

            migrationBuilder.AlterColumn<string>(
                name: "normalized_email",
                table: "account_login_history",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(24)",
                oldMaxLength: 24);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "accounts",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "normalized_email",
                table: "accounts",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE accounts SET email = lower(username) || '@legacy.invalid', " +
                "normalized_email = upper(username) || '@LEGACY.INVALID'");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "accounts",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "normalized_email",
                table: "accounts",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_normalized_email",
                table: "accounts",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_accounts_normalized_email",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "email",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "normalized_email",
                table: "accounts");

            migrationBuilder.Sql(
                "UPDATE account_login_history SET normalized_email = left(normalized_email, 24)");

            migrationBuilder.AlterColumn<string>(
                name: "normalized_email",
                table: "account_login_history",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "account_login_history",
                newName: "normalized_username");
        }
    }
}
