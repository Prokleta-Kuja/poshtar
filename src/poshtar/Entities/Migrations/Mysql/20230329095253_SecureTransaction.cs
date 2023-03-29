using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class SecureTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "complete",
                table: "transactions",
                newName: "secure");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "secure",
                table: "transactions",
                newName: "complete");
        }
    }
}
