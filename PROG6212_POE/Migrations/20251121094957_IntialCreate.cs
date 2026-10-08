using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PROG6212_POE.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HourCap = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    claimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hourlyRate = table.Column<int>(type: "int", nullable: false),
                    hoursWorked = table.Column<int>(type: "int", nullable: false),
                    claimNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstApproval = table.Column<bool>(type: "bit", nullable: false),
                    claimStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    dateOfClaim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    documentName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    document = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    userId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.claimId);
                    table.ForeignKey(
                        name: "FK_Claims_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "HourCap", "HourlyRate", "Name", "Password", "Role", "Surname" },
                values: new object[,]
                {
                    { 1, "twill@gmail.com", null, null, "Thomas", "ThowillMas361", "HR", "Williams" },
                    { 2, "mad@gmail.com", null, null, "Madeline", "SmithEline953", "HR", "Smith" },
                    { 3, "doe@gmail.com", 40, 150m, "John", "JoeDohn145", "Lecturer", "Doe" },
                    { 4, "samson@gmail.com", null, null, "Sam", "SSson528", "Programme Coordinator", "Sampson" },
                    { 5, "AA@gmail.com", null, null, "Alvin", "vinny216", "Academic Manager", "Avin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_userId",
                table: "Claims",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
