using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Totp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "otp_key",
                table: "users",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "otp_key",
                table: "users");
        }
    }
}
