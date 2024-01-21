using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Server.Migrations
{
    /// <inheritdoc />
    public partial class GroupMemberStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupMemberStatus",
                table: "GroupMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupMemberStatus",
                table: "GroupMembers");
        }
    }
}
