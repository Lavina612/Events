using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsRestApi.Services.EventService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialEventDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    available_seats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.CheckConstraint("CK_Event_AvailableSeats", "available_seats >= 0 AND available_seats <= total_seats");
                    table.CheckConstraint("CK_Event_Dates", "start_at < end_at");
                    table.CheckConstraint("CK_Event_TotalSeats", "total_seats > 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
