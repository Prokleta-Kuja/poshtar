using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class TransactionCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "submission",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "submission",
                table: "transactions");
        }
    }
}
