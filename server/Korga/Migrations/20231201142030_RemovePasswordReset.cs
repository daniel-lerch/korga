using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Migrations
{
    /// <inheritdoc />
    public partial class RemovePasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    Token = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Expiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Uid = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.Token);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
