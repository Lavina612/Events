using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsRestApi.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "bookings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 2. Накатываем внешний ключ. PostgreSQL сделает это мгновенно, 
            // так как все user_id ссылаются на реально существующего пользователя.
            migrationBuilder.AddForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Откат изменений в обратном порядке
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "bookings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: false);
        }
    }
}
