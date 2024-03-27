using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class MovedNotesAndNotifyFromProcessingToExp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ExperimentProcessing");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Experiment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyUser",
                table: "Experiment",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Experiment");

            migrationBuilder.DropColumn(
                name: "NotifyUser",
                table: "Experiment");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ExperimentProcessing",
                type: "text",
                nullable: true);
        }
    }
}
