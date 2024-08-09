using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Migrations
{
    /// <inheritdoc />
    public partial class Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "People",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "PermittedSendersId",
                table: "DistributionLists",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonFilterListId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Permissions_PersonFilterLists_PersonFilterListId",
                        column: x => x.PersonFilterListId,
                        principalTable: "PersonFilterLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_People_Email",
                table: "People",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_PermittedSendersId",
                table: "DistributionLists",
                column: "PermittedSendersId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PersonFilterListId",
                table: "Permissions",
                column: "PersonFilterListId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedSendersId",
                table: "DistributionLists",
                column: "PermittedSendersId",
                principalTable: "PersonFilterLists",
                principalColumn: "Id");

            migrationBuilder.Sql("INSERT INTO `PersonFilterLists` VALUES (NULL)");
            migrationBuilder.Sql("INSERT INTO `Permissions` VALUES (\"Permissions_View\", LAST_INSERT_ID())");
            migrationBuilder.Sql("INSERT INTO `PersonFilterLists` VALUES (NULL)");
            migrationBuilder.Sql("INSERT INTO `Permissions` VALUES (\"Permissions_Admin\", LAST_INSERT_ID())");
            migrationBuilder.Sql("INSERT INTO `PersonFilterLists` VALUES (NULL)");
            migrationBuilder.Sql("INSERT INTO `Permissions` VALUES (\"DistributionLists_View\", LAST_INSERT_ID())");
            migrationBuilder.Sql("INSERT INTO `PersonFilterLists` VALUES (NULL)");
            migrationBuilder.Sql("INSERT INTO `Permissions` VALUES (\"DistributionLists_Admin\", LAST_INSERT_ID())");
            migrationBuilder.Sql("INSERT INTO `PersonFilterLists` VALUES (NULL)");
            migrationBuilder.Sql("INSERT INTO `Permissions` VALUES (\"ServiceHistory_View\", LAST_INSERT_ID())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"DELETE FROM `PersonFilterLists` WHERE `Id` IN
(SELECT `PersonFilterListId` FROM `Permissions`)");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_People_Email",
                table: "People");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedSendersId",
                table: "DistributionLists");

            migrationBuilder.Sql(
@"DELETE FROM `PersonFilterLists` WHERE `Id` IN
(SELECT `PermittedSendersId` FROM `DistributionLists` WHERE `PermittedSendersId` IS NOT NULL)");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_PermittedSendersId",
                table: "DistributionLists");

            migrationBuilder.DropColumn(
                name: "PermittedSendersId",
                table: "DistributionLists");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "People",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
