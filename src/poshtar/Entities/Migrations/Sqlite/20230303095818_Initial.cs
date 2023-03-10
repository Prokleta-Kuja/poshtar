using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    friendly_name = table.Column<string>(type: "TEXT", nullable: true),
                    xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "domains",
                columns: table => new
                {
                    domain_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    host = table.Column<string>(type: "TEXT", nullable: false),
                    port = table.Column<int>(type: "INTEGER", nullable: false),
                    is_secure = table.Column<bool>(type: "INTEGER", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    disabled = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domains", x => x.domain_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    is_master = table.Column<bool>(type: "INTEGER", nullable: false),
                    quota = table.Column<int>(type: "INTEGER", nullable: true),
                    salt = table.Column<string>(type: "TEXT", nullable: false),
                    hash = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<string>(type: "TEXT", nullable: false),
                    disabled = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    address_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    domain_id = table.Column<int>(type: "INTEGER", nullable: false),
                    pattern = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    expression = table.Column<string>(type: "TEXT", nullable: true, computedColumnSql: "CASE type WHEN 0 THEN pattern WHEN 1 THEN pattern || '%' WHEN 2 THEN '%' || pattern ELSE NULL END", stored: true),
                    disabled = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.address_id);
                    table.ForeignKey(
                        name: "fk_addresses_domains_domain_id",
                        column: x => x.domain_id,
                        principalTable: "domains",
                        principalColumn: "domain_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "address_user",
                columns: table => new
                {
                    addresses_address_id = table.Column<int>(type: "INTEGER", nullable: false),
                    users_user_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address_user", x => new { x.addresses_address_id, x.users_user_id });
                    table.ForeignKey(
                        name: "fk_address_user_addresses_addresses_address_id",
                        column: x => x.addresses_address_id,
                        principalTable: "addresses",
                        principalColumn: "address_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_address_user_users_users_user_id",
                        column: x => x.users_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_address_user_users_user_id",
                table: "address_user",
                column: "users_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_addresses_domain_id",
                table: "addresses",
                column: "domain_id");

            migrationBuilder.CreateIndex(
                name: "ix_domains_name",
                table: "domains",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address_user");

            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "domains");
        }
    }
}
