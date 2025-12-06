using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class ChurchQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropTable(
                name: "DepartmentMembers");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "PersonFilters");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "GroupRoles");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "PersonFilterLists");

            migrationBuilder.DropTable(
                name: "GroupStatuses");

            migrationBuilder.DropTable(
                name: "GroupTypes");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.DropColumn(
                name: "PermittedRecipientsId",
                table: "DistributionLists");

            migrationBuilder.AddColumn<string>(
                name: "RecipientsQuery",
                table: "DistributionLists",
                type: "longtext",
                nullable: false,
                defaultValue: "null")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxEmails_DistributionLists_DistributionListId",
                table: "InboxEmails");

            migrationBuilder.AddForeignKey(
                name: "FK_InboxEmails_DistributionLists_DistributionListId",
                table: "InboxEmails",
                column: "DistributionListId",
                principalTable: "DistributionLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboxEmails_DistributionLists_DistributionListId",
                table: "InboxEmails");

            migrationBuilder.AddForeignKey(
                name: "FK_InboxEmails_DistributionLists_DistributionListId",
                table: "InboxEmails",
                column: "DistributionListId",
                principalTable: "DistributionLists",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "RecipientsQuery",
                table: "DistributionLists");

            migrationBuilder.AddColumn<long>(
                name: "PermittedRecipientsId",
                table: "DistributionLists",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupStatuses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GroupTypeId = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupRoles_GroupTypes_GroupTypeId",
                        column: x => x.GroupTypeId,
                        principalTable: "GroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GroupStatusId = table.Column<int>(type: "int", nullable: false),
                    GroupTypeId = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_GroupStatuses_GroupStatusId",
                        column: x => x.GroupStatusId,
                        principalTable: "GroupStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groups_GroupTypes_GroupTypeId",
                        column: x => x.GroupTypeId,
                        principalTable: "GroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DepartmentMembers",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentMembers", x => new { x.PersonId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_DepartmentMembers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentMembers_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupRoleId = table.Column<int>(type: "int", nullable: false),
                    GroupMemberStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.PersonId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupMembers_GroupRoles_GroupRoleId",
                        column: x => x.GroupRoleId,
                        principalTable: "GroupRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PersonFilters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PersonFilterListId = table.Column<long>(type: "bigint", nullable: false),
                    Discriminator = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EqualityKey = table.Column<string>(type: "varchar(255)", nullable: false, computedColumnSql: "\r\nCONCAT(\r\n    LPAD(HEX(IFNULL(`GroupId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupRoleId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupTypeId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`PersonId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`StatusId`, 0)), 8, '0'))\r\n")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    GroupRoleId = table.Column<int>(type: "int", nullable: true),
                    GroupTypeId = table.Column<int>(type: "int", nullable: true),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonFilters_GroupRoles_GroupRoleId",
                        column: x => x.GroupRoleId,
                        principalTable: "GroupRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PersonFilters_GroupTypes_GroupTypeId",
                        column: x => x.GroupTypeId,
                        principalTable: "GroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonFilters_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonFilters_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonFilters_PersonFilterLists_PersonFilterListId",
                        column: x => x.PersonFilterListId,
                        principalTable: "PersonFilterLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonFilters_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentMembers_DepartmentId",
                table: "DepartmentMembers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupRoleId",
                table: "GroupMembers",
                column: "GroupRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoles_GroupTypeId",
                table: "GroupRoles",
                column: "GroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupStatusId",
                table: "Groups",
                column: "GroupStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupTypeId",
                table: "Groups",
                column: "GroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_People_StatusId",
                table: "People",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_GroupId",
                table: "PersonFilters",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_GroupRoleId",
                table: "PersonFilters",
                column: "GroupRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_GroupTypeId",
                table: "PersonFilters",
                column: "GroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_PersonFilterListId_Discriminator_EqualityKey",
                table: "PersonFilters",
                columns: new[] { "PersonFilterListId", "Discriminator", "EqualityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_PersonId",
                table: "PersonFilters",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_StatusId",
                table: "PersonFilters",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionLists_PersonFilterLists_PermittedRecipientsId",
                table: "DistributionLists",
                column: "PermittedRecipientsId",
                principalTable: "PersonFilterLists",
                principalColumn: "Id");
        }
    }
}
