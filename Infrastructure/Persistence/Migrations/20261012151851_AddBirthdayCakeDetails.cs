using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthdayCakeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CakeDescription",
                table: "Bookings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CakeImageContentType",
                table: "Bookings",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "CakeImageData",
                table: "Bookings",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CakeDescription",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CakeImageContentType",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CakeImageData",
                table: "Bookings");
        }
    }
}
