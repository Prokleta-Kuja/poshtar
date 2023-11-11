using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poshtar.Entities.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Calendars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendars",
                columns: table => new
                {
                    calendar_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    display_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendars", x => x.calendar_id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_objects",
                columns: table => new
                {
                    calendar_object_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    calendar_id = table.Column<int>(type: "INTEGER", nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    first_occurence = table.Column<long>(type: "INTEGER", nullable: true),
                    last_occurence = table.Column<long>(type: "INTEGER", nullable: true),
                    file_name = table.Column<string>(type: "TEXT", nullable: false),
                    modified = table.Column<long>(type: "INTEGER", nullable: false),
                    deleted = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_objects", x => x.calendar_object_id);
                    table.ForeignKey(
                        name: "fk_calendar_objects_calendars_calendar_id",
                        column: x => x.calendar_id,
                        principalTable: "calendars",
                        principalColumn: "calendar_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "calendar_users",
                columns: table => new
                {
                    calendar_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    can_write = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_users", x => new { x.calendar_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_calendar_users_calendars_calendar_id",
                        column: x => x.calendar_id,
                        principalTable: "calendars",
                        principalColumn: "calendar_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_calendar_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calendar_objects_calendar_id",
                table: "calendar_objects",
                column: "calendar_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_users_user_id",
                table: "calendar_users",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_objects");

            migrationBuilder.DropTable(
                name: "calendar_users");

            migrationBuilder.DropTable(
                name: "calendars");
        }
    }
}
