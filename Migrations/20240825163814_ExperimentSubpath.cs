using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class ExperimentSubpath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubPath",
                table: "ExperimentStorage",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubPath",
                table: "ExperimentStorage");
        }
    }
}
