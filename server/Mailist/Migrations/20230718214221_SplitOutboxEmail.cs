using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class SplitOutboxEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SentEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    InboxEmailId = table.Column<long>(type: "bigint", nullable: true),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentSize = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SentEmails_InboxEmails_InboxEmailId",
                        column: x => x.InboxEmailId,
                        principalTable: "InboxEmails",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Make UniqueId column nullable and change zero values to NULL to avoid conflicts when creating a unqiue index later
            migrationBuilder.AlterColumn<uint?>(
                name: "UniqueId",
                table: "InboxEmails",
                type: "int unsigned",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.Sql("UPDATE `InboxEmails` SET `UniqueId` = NULL WHERE `UniqueId` = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmails_ProcessingCompletedTime",
                table: "InboxEmails",
                column: "ProcessingCompletedTime");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmails_UniqueId",
                table: "InboxEmails",
                column: "UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentEmails_DeliveryTime",
                table: "SentEmails",
                column: "DeliveryTime");

            migrationBuilder.CreateIndex(
                name: "IX_SentEmails_InboxEmailId",
                table: "SentEmails",
                column: "InboxEmailId");

            // Migrate archive data by creating new SentEmails for each OutboxEmails which has already been delivered.
            migrationBuilder.Sql(
@"INSERT INTO `SentEmails`
(`Id`, `InboxEmailId`, `EmailAddress`, `ContentSize`, `ErrorMessage`, `DeliveryTime`)
SELECT `Id`, `InboxEmailId`, `EmailAddress`, LENGTH(`Content`), `ErrorMessage`, `DeliveryTime`
FROM `OutboxEmails`
WHERE `DeliveryTime` <> ""0001-01-01 00:00:00""");

            // Delete OutboxEmails which have already been delivered because their information is now stored in SentEmails.
            migrationBuilder.Sql(
@"DELETE FROM `OutboxEmails`
WHERE `DeliveryTime` <> ""0001-01-01 00:00:00""");

            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "OutboxEmails");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "OutboxEmails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SentEmails");

            migrationBuilder.DropIndex(
                name: "IX_InboxEmails_ProcessingCompletedTime",
                table: "InboxEmails");

            migrationBuilder.DropIndex(
                name: "IX_InboxEmails_UniqueId",
                table: "InboxEmails");

            migrationBuilder.AlterColumn<uint>(
                name: "UniqueId",
                table: "InboxEmails",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(uint),
                oldType: "int unsigned",
                oldNullable: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryTime",
                table: "OutboxEmails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "OutboxEmails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
