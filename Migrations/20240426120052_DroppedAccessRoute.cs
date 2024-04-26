using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class DroppedAccessRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessRoute",
                table: "Experiment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessRoute",
                table: "Experiment",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }
    }
}
