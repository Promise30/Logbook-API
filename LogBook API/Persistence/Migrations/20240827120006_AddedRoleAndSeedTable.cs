using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogBook_API.Migrations
{
    /// <inheritdoc />
    public partial class AddedRoleAndSeedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3a5f73c0-db6d-40a2-ae76-8c44c946d72d", "1", "User", "USER" },
                    { "60b66eb1-1d55-4cbf-9cda-796c1730d4b6", "2", "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3a5f73c0-db6d-40a2-ae76-8c44c946d72d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "60b66eb1-1d55-4cbf-9cda-796c1730d4b6");
        }
    }
}
