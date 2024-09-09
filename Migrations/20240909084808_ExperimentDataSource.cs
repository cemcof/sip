using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class ExperimentDataSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExperimentDataSource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExperimentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDirectory = table.Column<string>(type: "text", nullable: true),
                    SourcePatternsStr = table.Column<string>(type: "text", nullable: true),
                    KeepSourceFiles = table.Column<bool>(type: "boolean", nullable: false),
                    CleanAfter = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DtCleaned = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentDataSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperimentDataSource_Experiment_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "Experiment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentDataSource_ExperimentId",
                table: "ExperimentDataSource",
                column: "ExperimentId",
                unique: true);
            
            // Before dropping the colums, copy them to new table, posgresql, create guid
            migrationBuilder.Sql(@"
                INSERT INTO ""ExperimentDataSource"" (""Id"", ""ExperimentId"", ""SourceDirectory"", ""SourcePatternsStr"", ""KeepSourceFiles"", ""CleanAfter"", ""DtCleaned"")
                SELECT gen_random_uuid(), ""ExperimentId"", ""SourceDirectory"", ""SourcePatternsStr"", ""KeepSourceFiles"", '7.00:00:00', NULL
                FROM ""ExperimentStorage""");
            
            migrationBuilder.DropColumn(
                name: "Clean",
                table: "ExperimentStorage");

            migrationBuilder.DropColumn(
                name: "KeepSourceFiles",
                table: "ExperimentStorage");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "ExperimentStorage");

            migrationBuilder.DropColumn(
                name: "SourceDirectory",
                table: "ExperimentStorage");

            migrationBuilder.DropColumn(
                name: "SourcePatternsStr",
                table: "ExperimentStorage");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Clean",
                table: "ExperimentStorage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KeepSourceFiles",
                table: "ExperimentStorage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ExperimentStorage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDirectory",
                table: "ExperimentStorage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourcePatternsStr",
                table: "ExperimentStorage",
                type: "text",
                nullable: false,
                defaultValue: "");
            
            // Before dropping table, copy data to the new columns, postgres, update statement
            // TODO - for now irreversible
            
            migrationBuilder.DropTable(
                name: "ExperimentDataSource");

        }
    }
}
