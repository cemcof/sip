using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeletingExpLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Log_Experiment_ExperimentId",
                table: "Log");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_Experiment_ExperimentId",
                table: "Log",
                column: "ExperimentId",
                principalTable: "Experiment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Log_Experiment_ExperimentId",
                table: "Log");

            migrationBuilder.AddForeignKey(
                name: "FK_Log_Experiment_ExperimentId",
                table: "Log",
                column: "ExperimentId",
                principalTable: "Experiment",
                principalColumn: "Id");
        }
    }
}
