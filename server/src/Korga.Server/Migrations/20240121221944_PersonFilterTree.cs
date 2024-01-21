using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Server.Migrations
{
    /// <inheritdoc />
    public partial class PersonFilterTree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupTypeId",
                table: "PersonFilters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "PersonFilters",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PermittedRecipientsId",
                table: "DistributionLists",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_GroupTypeId",
                table: "PersonFilters",
                column: "GroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_ParentId",
                table: "PersonFilters",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_PersonFilters_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId",
                principalTable: "PersonFilters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_GroupTypes_GroupTypeId",
                table: "PersonFilters",
                column: "GroupTypeId",
                principalTable: "GroupTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_PersonFilters_ParentId",
                table: "PersonFilters",
                column: "ParentId",
                principalTable: "PersonFilters",
                principalColumn: "Id");

            // Create LogicalOr filters for every distribution list that has a person filter
            migrationBuilder.Sql(
@"INSERT INTO `PersonFilters` (`DistributionListId`, `Discriminator`)
SELECT DISTINCT(`DistributionListId`), ""LogicalOr"" FROM `PersonFilters`");

            // These UPDATE SELECT queries might not work with databases other than MySQL/MariaDB
            // Furthermore, the inner query must not filter rows because of optimization problems:
            // https://dev.mysql.com/doc/refman/8.0/en/update.html

            // Set ParentId for all original person filters
            migrationBuilder.Sql(
@"UPDATE `PersonFilters` AS `original`,
        (SELECT `Id`, `DistributionListId`, `Discriminator` FROM `PersonFilters`) AS `parent`
SET `original`.`ParentId` = `parent`.`Id`
WHERE `parent`.`Discriminator` = ""LogicalOr""
    AND `parent`.`DistributionListId` = `original`.`DistributionListId`
    AND `original`.`Discriminator` <> ""LogicalOr""");

            // Set permitted recipients
            migrationBuilder.Sql(
@"UPDATE `DistributionLists` AS `dist`,
        (SELECT `Id`, `DistributionListId`, `Discriminator` FROM `PersonFilters`) AS `filter`
SET `dist`.`PermittedRecipientsId` = `filter`.`Id`
WHERE `filter`.`Discriminator` = ""LogicalOr""
    AND `filter`.`DistributionListId` = `dist`.`Id`");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_DistributionLists_DistributionListId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_DistributionListId",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "DistributionListId",
                table: "PersonFilters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_PersonFilters_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_GroupTypes_GroupTypeId",
                table: "PersonFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_PersonFilters_ParentId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_GroupTypeId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_ParentId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropColumn(
                name: "GroupTypeId",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.AddColumn<long>(
                name: "DistributionListId",
                table: "PersonFilters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_DistributionListId",
                table: "PersonFilters",
                column: "DistributionListId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_DistributionLists_DistributionListId",
                table: "PersonFilters",
                column: "DistributionListId",
                principalTable: "DistributionLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
