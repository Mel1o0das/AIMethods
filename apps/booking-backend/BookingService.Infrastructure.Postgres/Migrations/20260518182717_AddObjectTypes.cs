using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "object_type_id",
                table: "booking_objects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "object_types",
                columns: table => new
                {
                    object_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_object_types", x => x.object_type_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_objects_object_type_id",
                table: "booking_objects",
                column: "object_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_object_types_name",
                table: "object_types",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "object_types");

            migrationBuilder.DropIndex(
                name: "IX_booking_objects_object_type_id",
                table: "booking_objects");

            migrationBuilder.DropColumn(
                name: "object_type_id",
                table: "booking_objects");
        }
    }
}
