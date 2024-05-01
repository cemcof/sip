using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class AddedNodeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Node",
                table: "ExperimentStorage",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Node",
                table: "ExperimentPublication",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Node",
                table: "ExperimentProcessing",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Node",
                table: "ExperimentStorage");

            migrationBuilder.DropColumn(
                name: "Node",
                table: "ExperimentPublication");

            migrationBuilder.AlterColumn<string>(
                name: "Node",
                table: "ExperimentProcessing",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
