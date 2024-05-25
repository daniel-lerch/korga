using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korga.Migrations
{
    /// <inheritdoc />
    public partial class PersonFilterList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These annotation changes seem to be part of a new EF Core version
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "DistributionLists",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "PersonFilters",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "PersonFilters",
                type: "varchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "InboxEmails",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "OutboxEmails",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            // Add group type first because it is a trivial operation
            migrationBuilder.AddColumn<int>(
                name: "GroupTypeId",
                table: "PersonFilters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_GroupTypeId",
                table: "PersonFilters",
                column: "GroupTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_GroupTypes_GroupTypeId",
                table: "PersonFilters",
                column: "GroupTypeId",
                principalTable: "GroupTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Now that our columns are complete add the new equality key
            migrationBuilder.AddColumn<string>(
                name: "EqualityKey",
                table: "PersonFilters",
                type: "varchar(255)",
                nullable: false,
                computedColumnSql: "\r\nCONCAT(\r\n    LPAD(HEX(IFNULL(`GroupId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupRoleId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupTypeId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`PersonId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`StatusId`, 0)), 8, '0'))\r\n")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_PersonFilterListId_Discriminator_EqualityKey",
                table: "PersonFilters",
                columns: ["DistributionListId", "Discriminator", "EqualityKey"],
                unique: true);

            // Create new table first to migrate filter mappings later on
            migrationBuilder.CreateTable(
                name: "PersonFilterLists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonFilterLists", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Create a filter list for each distribution list that has a person filter
            migrationBuilder.Sql(
@"INSERT INTO `PersonFilterLists` (`Id`)
SELECT DISTINCT(`DistributionListId`) FROM `PersonFilters`");

            // Change person filter's foreign key to point to filter lists instead of distribution lists
            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_DistributionLists_DistributionListId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_DistributionListId",
                table: "PersonFilters");

            migrationBuilder.RenameColumn(
                name: "DistributionListId",
                table: "PersonFilters",
                newName: "PersonFilterListId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_PersonFilterLists_PersonFilterListId",
                table: "PersonFilters",
                column: "PersonFilterListId",
                principalTable: "PersonFilterLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Add new column to distribution lists to link filter lists
            migrationBuilder.AddColumn<long>(
                name: "PermittedRecipientsId",
                table: "DistributionLists",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId",
                principalTable: "PersonFilterLists",
                principalColumn: "Id");

            // Finally fill permitted recipients with the previously migrated filter list IDs

            // This UPDATE SELECT query might not work with databases other than MySQL/MariaDB
            // Furthermore, the inner query must not filter rows because of optimization problems:
            // https://dev.mysql.com/doc/refman/8.0/en/update.html

            migrationBuilder.Sql(
@"UPDATE `DistributionLists` AS `dist`,
        (SELECT `Id` FROM `PersonFilterLists`) AS `filterlist`
SET `dist`.`PermittedRecipientsId` = `filterlist`.`Id`
WHERE `dist`.`Id` = `filterlist`.`Id`");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove equality key first because it depends on new columns which are removed later
            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_DistributionListId",
                table: "PersonFilters",
                column: "PersonFilterListId");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_PersonFilterListId_Discriminator_EqualityKey",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "EqualityKey",
                table: "PersonFilters");

            // Then remove group type because it is a trivial operation
            migrationBuilder.Sql(@"DELETE FROM `PersonFilters` WHERE `Discriminator` = ""GroupTypeFilter""");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_GroupTypes_GroupTypeId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_GroupTypeId",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "GroupTypeId",
                table: "PersonFilters");

            // Remove all filter lists that are not referenced by a distribution list
            migrationBuilder.Sql(
@"DELETE FROM `PersonFilterLists`
WHERE `Id` NOT IN
    (SELECT DISTINCT `PermittedRecipientsId` FROM `DistributionLists`
    WHERE `PermittedRecipientsId` IS NOT NULL)");

            // Rename foreign key and make it point to distribution lists again
            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_PersonFilterLists_PersonFilterListId",
                table: "PersonFilters");

            migrationBuilder.RenameColumn(
                name: "PersonFilterListId",
                table: "PersonFilters",
                newName: "DistributionListId");

            // This UPDATE SELECT query might not work with databases other than MySQL/MariaDB
            // Furthermore, the inner query must not filter rows because of optimization problems:
            // https://dev.mysql.com/doc/refman/8.0/en/update.html

            migrationBuilder.Sql(
@"UPDATE `PersonFilters` AS `filter`,
        (SELECT `Id`,`PermittedRecipientsId` FROM `DistributionLists`) AS `dist`
SET `filter`.`DistributionListId` = `dist`.`Id`
WHERE `filter`.`DistributionListId` = `dist`.`PermittedRecipientsId`");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_DistributionLists_DistributionListId",
                table: "PersonFilters",
                column: "DistributionListId",
                principalTable: "DistributionLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Now that filters are linked to distribution lists again, we can remove the filter list table
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropColumn(
                name: "PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropTable(
                name: "PersonFilterLists");

            // The annotation changes from a new EF Core version must not be reverted
            // Otherwise the auto_increment attribute would be lost

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "PersonFilters",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(21)",
                oldMaxLength: 21)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
