using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class InboxOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailRecipients");

            migrationBuilder.CreateTable(
                name: "InboxEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DistributionListId = table.Column<long>(type: "bigint", nullable: true),
                    UniqueId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    From = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sender = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReplyTo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    To = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Receiver = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Header = table.Column<byte[]>(type: "longblob", nullable: true),
                    Body = table.Column<byte[]>(type: "longblob", nullable: true),
                    DownloadTime = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    ProcessingCompletedTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxEmails_DistributionLists_DistributionListId",
                        column: x => x.DistributionListId,
                        principalTable: "DistributionLists",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OutboxEmails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InboxEmailId = table.Column<long>(type: "bigint", nullable: true),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<byte[]>(type: "longblob", nullable: false),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeliveryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxEmails_InboxEmails_InboxEmailId",
                        column: x => x.InboxEmailId,
                        principalTable: "InboxEmails",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InboxEmails_DistributionListId",
                table: "InboxEmails",
                column: "DistributionListId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEmails_InboxEmailId",
                table: "OutboxEmails",
                column: "InboxEmailId");

            // Insert data from old Emails table into new InboxEmails table. EmailRecipients will not be migrated.
            migrationBuilder.Sql(
@"INSERT INTO `InboxEmails` 
(`Id`, `DistributionListId`, `UniqueId`, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `ProcessingCompletedTime`)
SELECT `Id`, `DistributionListId`, 0, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `RecipientsFetchTime`
FROM `Emails`");

            migrationBuilder.DropTable(
                name: "Emails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxEmails");

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DistributionListId = table.Column<long>(type: "bigint", nullable: true),
                    Body = table.Column<byte[]>(type: "longblob", nullable: false),
                    DownloadTime = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    From = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Receiver = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientsFetchTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Sender = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    To = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emails_DistributionLists_DistributionListId",
                        column: x => x.DistributionListId,
                        principalTable: "DistributionLists",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailRecipients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmailId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailRecipients_Emails_EmailId",
                        column: x => x.EmailId,
                        principalTable: "Emails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRecipients_EmailId",
                table: "EmailRecipients",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_DistributionListId",
                table: "Emails",
                column: "DistributionListId");

            // Insert data from new InboxEmails table into the old recreated Emails table. OutboxEmails will not be migrated into EmailRecipients.
            migrationBuilder.Sql(
@"INSERT INTO `Emails`
(`Id`, `DistributionListId`, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `RecipientsFetchTime`)
SELECT `Id`, `DistributionListId`, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `ProcessingCompletedTime`
FROM `InboxEmails`");

            migrationBuilder.DropTable(
                name: "InboxEmails");
        }
    }
}
