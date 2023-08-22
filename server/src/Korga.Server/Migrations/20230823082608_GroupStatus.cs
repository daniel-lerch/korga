using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Server.Migrations
{
    /// <inheritdoc />
    public partial class GroupStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStatuses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("INSERT INTO `GroupStatuses` VALUES (1, \"default\", \"0001-01-01 00:00:00\")");

            migrationBuilder.AddColumn<int>(
                name: "GroupStatusId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupStatusId",
                table: "Groups",
                column: "GroupStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupStatuses_GroupStatusId",
                table: "Groups",
                column: "GroupStatusId",
                principalTable: "GroupStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Status",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "People",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "GroupTypes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Groups",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "GroupRoles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Departments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupStatuses_GroupStatusId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "GroupStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Groups_GroupStatusId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "GroupStatusId",
                table: "Groups");

            migrationBuilder.Sql("DELETE FROM `Status` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Status");

            migrationBuilder.Sql("DELETE FROM `People` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "People");

            migrationBuilder.Sql("DELETE FROM `GroupTypes` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "GroupTypes");

            migrationBuilder.Sql("DELETE FROM `Groups` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Groups");

            migrationBuilder.Sql("DELETE FROM `GroupRoles` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "GroupRoles");

            migrationBuilder.Sql("DELETE FROM `Departments` WHERE `DeletionTime` <> \"0001-01-01 00:00:00\"");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Departments");
        }
    }
}
