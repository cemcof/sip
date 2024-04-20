using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class AddingWorkflowReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowRef",
                table: "ExperimentProcessing",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowRef",
                table: "ExperimentProcessing");
        }
    }
}
