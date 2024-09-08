using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrganizationToExperiment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationUserId",
                table: "Experiment",
                type: "character varying(128)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Experiment_OrganizationUserId",
                table: "Experiment",
                column: "OrganizationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Experiment_Organization_OrganizationUserId",
                table: "Experiment",
                column: "OrganizationUserId",
                principalTable: "Organization",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Experiment_Organization_OrganizationUserId",
                table: "Experiment");

            migrationBuilder.DropIndex(
                name: "IX_Experiment_OrganizationUserId",
                table: "Experiment");

            migrationBuilder.DropColumn(
                name: "OrganizationUserId",
                table: "Experiment");
        }
    }
}
