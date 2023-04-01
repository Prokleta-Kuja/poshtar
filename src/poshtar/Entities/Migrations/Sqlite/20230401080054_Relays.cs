using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Relays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_recipients_users_user_id",
                table: "recipients");

            migrationBuilder.DropColumn(
                name: "host",
                table: "domains");

            migrationBuilder.DropColumn(
                name: "password",
                table: "domains");

            migrationBuilder.DropColumn(
                name: "port",
                table: "domains");

            migrationBuilder.DropColumn(
                name: "username",
                table: "domains");

            migrationBuilder.AddColumn<int>(
                name: "relay_id",
                table: "domains",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "relays",
                columns: table => new
                {
                    relay_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    host = table.Column<string>(type: "TEXT", nullable: false),
                    port = table.Column<int>(type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    disabled = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relays", x => x.relay_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domains_relay_id",
                table: "domains",
                column: "relay_id");

            migrationBuilder.CreateIndex(
                name: "ix_relays_name",
                table: "relays",
                column: "name");

            migrationBuilder.AddForeignKey(
                name: "fk_domains_relays_relay_id",
                table: "domains",
                column: "relay_id",
                principalTable: "relays",
                principalColumn: "relay_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_recipients_users_user_id",
                table: "recipients",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_domains_relays_relay_id",
                table: "domains");

            migrationBuilder.DropForeignKey(
                name: "fk_recipients_users_user_id",
                table: "recipients");

            migrationBuilder.DropTable(
                name: "relays");

            migrationBuilder.DropIndex(
                name: "ix_domains_relay_id",
                table: "domains");

            migrationBuilder.DropColumn(
                name: "relay_id",
                table: "domains");

            migrationBuilder.AddColumn<string>(
                name: "host",
                table: "domains",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "domains",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "port",
                table: "domains",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "domains",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_recipients_users_user_id",
                table: "recipients",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id");
        }
    }
}
