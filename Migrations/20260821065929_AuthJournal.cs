using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_P42.Migrations
{
    /// <inheritdoc />
    public partial class AuthJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthJournals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Login = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dk = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsOk = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthJournals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthJournals");
        }
    }
}
