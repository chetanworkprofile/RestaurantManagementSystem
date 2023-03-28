using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    foodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    foodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    timeToPrepare = table.Column<TimeSpan>(type: "time", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pathToPic = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.foodId);
                });

            migrationBuilder.CreateTable(
                name: "OrderFoodMap",
                columns: table => new
                {
                    mapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    orderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    foodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFoodMap", x => x.mapId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    orderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    totalPrice = table.Column<int>(type: "int", nullable: false),
                    orderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    rejectAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.orderId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone = table.Column<long>(type: "bigint", nullable: false),
                    userRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    passwordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    pathToProfilePic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    verificationOTP = table.Column<int>(type: "int", nullable: true),
                    otpUsableTill = table.Column<DateTime>(type: "datetime2", nullable: false),
                    verifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    isBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "OrderFoodMap");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
