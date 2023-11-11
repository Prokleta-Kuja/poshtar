using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace poshtar.Entities.Migrations.Postgres
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
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "relays",
                columns: table => new
                {
                    relay_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    port = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    disabled = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "domains",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "port",
                table: "domains",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "domains",
                type: "text",
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
