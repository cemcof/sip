using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
public partial class FixingTimespanTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Step 1: Add temporary columns to hold the converted values
        migrationBuilder.AddColumn<TimeSpan>(
            name: "NewExpirationPeriod",
            table: "ExperimentStorage",
            type: "interval",
            nullable: true); // Nullable for interim purposes

        migrationBuilder.AddColumn<TimeSpan>(
            name: "NewEmbargoPeriod",
            table: "ExperimentPublication",
            type: "interval",
            nullable: true); // Nullable for interim purposes

        // Step 2: Convert the existing string data into the new columns
        migrationBuilder.Sql(@"
            UPDATE ""ExperimentStorage""
            SET ""NewExpirationPeriod"" = ""ExpirationPeriod""::interval
            WHERE ""ExpirationPeriod"" IS NOT NULL;
        ");

        migrationBuilder.Sql(@"
            UPDATE ""ExperimentPublication""
            SET ""NewEmbargoPeriod"" = ""EmbargoPeriod""::interval
            WHERE ""EmbargoPeriod"" IS NOT NULL;
        ");

        // Step 3: Drop the old columns and rename the new ones
        migrationBuilder.DropColumn(
            name: "ExpirationPeriod",
            table: "ExperimentStorage");

        migrationBuilder.DropColumn(
            name: "EmbargoPeriod",
            table: "ExperimentPublication");

        migrationBuilder.RenameColumn(
            name: "NewExpirationPeriod",
            table: "ExperimentStorage",
            newName: "ExpirationPeriod");

        migrationBuilder.RenameColumn(
            name: "NewEmbargoPeriod",
            table: "ExperimentPublication",
            newName: "EmbargoPeriod");

        // Step 4: Make the new columns non-nullable
        migrationBuilder.AlterColumn<TimeSpan>(
            name: "ExpirationPeriod",
            table: "ExperimentStorage",
            type: "interval",
            nullable: false,
            oldClrType: typeof(TimeSpan),
            oldType: "interval",
            oldNullable: true);

        migrationBuilder.AlterColumn<TimeSpan>(
            name: "EmbargoPeriod",
            table: "ExperimentPublication",
            type: "interval",
            nullable: false,
            oldClrType: typeof(TimeSpan),
            oldType: "interval",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Step 1: Revert the columns to strings
        migrationBuilder.AddColumn<string>(
            name: "OldExpirationPeriod",
            table: "ExperimentStorage",
            type: "character varying(48)",
            nullable: true); // Nullable for interim purposes

        migrationBuilder.AddColumn<string>(
            name: "OldEmbargoPeriod",
            table: "ExperimentPublication",
            type: "character varying(48)",
            nullable: true); // Nullable for interim purposes

        // Step 2: Convert the data back from interval to string
        migrationBuilder.Sql(@"
            UPDATE ""ExperimentStorage""
            SET ""OldExpirationPeriod"" = ""ExpirationPeriod""::text
            WHERE ""ExpirationPeriod"" IS NOT NULL;
        ");

        migrationBuilder.Sql(@"
            UPDATE ""ExperimentPublication""
            SET ""OldEmbargoPeriod"" = ""EmbargoPeriod""::text
            WHERE ""EmbargoPeriod"" IS NOT NULL;
        ");

        // Step 3: Drop the interval columns and rename the old string columns back
        migrationBuilder.DropColumn(
            name: "ExpirationPeriod",
            table: "ExperimentStorage");

        migrationBuilder.DropColumn(
            name: "EmbargoPeriod",
            table: "ExperimentPublication");

        migrationBuilder.RenameColumn(
            name: "OldExpirationPeriod",
            table: "ExperimentStorage",
            newName: "ExpirationPeriod");

        migrationBuilder.RenameColumn(
            name: "OldEmbargoPeriod",
            table: "ExperimentPublication",
            newName: "EmbargoPeriod");

        // Step 4: Make the old string columns non-nullable again
        migrationBuilder.AlterColumn<string>(
            name: "ExpirationPeriod",
            table: "ExperimentStorage",
            type: "character varying(48)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(48)");

        migrationBuilder.AlterColumn<string>(
            name: "EmbargoPeriod",
            table: "ExperimentPublication",
            type: "character varying(48)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(48)");
    }
}

}
