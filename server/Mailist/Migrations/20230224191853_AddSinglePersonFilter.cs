using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class AddSinglePersonFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonId",
                table: "PersonFilters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonFilters_PersonId",
                table: "PersonFilters",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonFilters_People_PersonId",
                table: "PersonFilters",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonFilters_People_PersonId",
                table: "PersonFilters");

            migrationBuilder.DropIndex(
                name: "IX_PersonFilters_PersonId",
                table: "PersonFilters");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "PersonFilters");
        }
    }
}
