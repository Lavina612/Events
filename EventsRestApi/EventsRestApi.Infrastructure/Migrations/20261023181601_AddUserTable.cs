using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsRestApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "text", nullable: false),
                    hashed_password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_login",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "bookings",
                type: "uuid",
                nullable: true);

            var migrationUserId = Guid.NewGuid();

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "login", "hashed_password", "role" },
                values: new object[] { migrationUserId, "legacy_migration_user", "no_password_system_account", "User" });

            migrationBuilder.Sql($"UPDATE bookings SET user_id = '{migrationUserId}' WHERE user_id IS NULL;");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_user_id",
                table: "bookings",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "ix_bookings_user_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "bookings");
        }
    }
}
