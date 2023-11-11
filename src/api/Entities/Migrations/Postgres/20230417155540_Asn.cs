using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Postgres
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
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country_name",
                table: "transactions",
                type: "text",
                nullable: true);
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
