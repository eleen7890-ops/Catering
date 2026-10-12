using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuratedMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuratedMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PriceJod = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumGuests = table.Column<int>(type: "int", nullable: false),
                    MaximumGuests = table.Column<int>(type: "int", nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Vibe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Event = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuratedMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuratedMenuDishes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuratedMenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ServingDescription = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuratedMenuDishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuratedMenuDishes_CuratedMenus_CuratedMenuId",
                        column: x => x.CuratedMenuId,
                        principalTable: "CuratedMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuratedMenuDishes_CuratedMenuId_SortOrder",
                table: "CuratedMenuDishes",
                columns: new[] { "CuratedMenuId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CuratedMenus_IsPublished_Cuisine_Vibe_Event",
                table: "CuratedMenus",
                columns: new[] { "IsPublished", "Cuisine", "Vibe", "Event" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuratedMenuDishes");

            migrationBuilder.DropTable(
                name: "CuratedMenus");
        }
    }
}
