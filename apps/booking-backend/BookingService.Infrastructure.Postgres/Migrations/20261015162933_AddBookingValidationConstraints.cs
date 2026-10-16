using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingValidationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM bookings
                        WHERE start_date_time >= end_date_time
                           OR guests_count <= 0)
                    THEN
                        RAISE EXCEPTION 'Bookings contain invalid intervals or non-positive guest counts';
                    END IF;
                END $$;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_bookings_guests_count_positive",
                table: "bookings",
                sql: "guests_count > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_bookings_start_before_end",
                table: "bookings",
                sql: "start_date_time < end_date_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_bookings_guests_count_positive",
                table: "bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_bookings_start_before_end",
                table: "bookings");
        }
    }
}
