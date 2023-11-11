using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Mysql
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
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "submission",
                table: "transactions",
                type: "tinyint(1)",
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
