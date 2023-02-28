using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Migrations
{
    /// <inheritdoc />
    public partial class AddressExpressionComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "expression",
                table: "addresses",
                type: "text",
                nullable: true,
                computedColumnSql: "CASE type WHEN 0 THEN pattern WHEN 1 THEN pattern || '%' WHEN 2 THEN '%' || pattern ELSE NULL END",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expression",
                table: "addresses");
        }
    }
}
