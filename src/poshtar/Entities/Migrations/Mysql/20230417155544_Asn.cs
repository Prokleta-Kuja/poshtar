using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class Asn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "country",
                table: "transactions",
                newName: "country_code");

            migrationBuilder.AddColumn<string>(
                name: "asn",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "country_name",
                table: "transactions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "asn",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "country_name",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "country_code",
                table: "transactions",
                newName: "country");
        }
    }
}
