using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sip.Migrations
{
    /// <inheritdoc />
    public partial class IssuesWithComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Issue",
                type: "character varying(128)",
                nullable: false,
                defaultValue: "");
            
            // Primary key madness here 
            migrationBuilder.DropPrimaryKey(
                name: "PK_Issue",
                table: "Issue");

            
            // Set all organizationIds to value "CfCryo"
            migrationBuilder.Sql(@"
                UPDATE ""Issue""
                SET ""OrganizationId"" = 'CfCryo';");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Issue",
                table: "Issue",
                columns: new[] { "OrganizationId", "Id" });
            
            migrationBuilder.AddForeignKey(
                name: "FK_Issue_Organization_OrganizationId",
                table: "Issue",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            
            migrationBuilder.CreateTable(
                name: "IssueComment",
                columns: table => new
                {
                    _id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueOrganizationId = table.Column<string>(type: "character varying(128)", nullable: true),
                    IssueId = table.Column<string>(type: "character varying(10)", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<IPAddress>(type: "inet", maxLength: 42, nullable: true),
                    DtCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueComment", x => x._id);
                    table.ForeignKey(
                        name: "FK_IssueComment_AppUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssueComment_Issue_IssueOrganizationId_IssueId",
                        columns: x => new { x.IssueOrganizationId, x.IssueId },
                        principalTable: "Issue",
                        principalColumns: new[] { "OrganizationId", "Id" });
                });
            
            // Before dropping description, move it to IssueComment (postgres), but prepend "Description: " to the comment
            migrationBuilder.Sql(@"
                INSERT INTO ""IssueComment"" (""_id"", ""IssueOrganizationId"", ""IssueId"", ""AuthorId"", ""DtCreated"", ""Comment"")
                SELECT gen_random_uuid(), ""OrganizationId"", ""Id"", ""InitiatedById"", ""DtCreated"", 'Description: ' || ""Description""
                FROM ""Issue"";");
            
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Issue");
            
            // Before dropping SolutionDescription, move it to IssueComment (postgres), but prepend "Solution: " to the comment
            migrationBuilder.Sql(@"
                INSERT INTO ""IssueComment"" (""_id"", ""IssueOrganizationId"", ""IssueId"", ""AuthorId"", ""DtCreated"", ""Comment"")
                SELECT gen_random_uuid(), ""OrganizationId"", ""Id"", ""InitiatedById"", ""DtLastChange"", 'Solution: ' || ""SolutionDescription""
                FROM ""Issue"";");
            
            migrationBuilder.DropColumn(
                name: "SolutionDescription",
                table: "Issue");
            
            migrationBuilder.AlterColumn<int>(
                name: "NotifyIntervalDays",
                table: "Issue",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
            
            
            migrationBuilder.CreateIndex(
                name: "IX_IssueComment_AuthorId",
                table: "IssueComment",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComment_IssueOrganizationId_IssueId",
                table: "IssueComment",
                columns: new[] { "IssueOrganizationId", "IssueId" });

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    // Revert NotifyIntervalDays back to double
    migrationBuilder.AlterColumn<double>(
        name: "NotifyIntervalDays",
        table: "Issue",
        type: "double precision",
        nullable: false,
        oldClrType: typeof(int),
        oldType: "integer");

    // Add back SolutionDescription and restore its values from IssueComment
    migrationBuilder.AddColumn<string>(
        name: "SolutionDescription",
        table: "Issue",
        type: "text",
        nullable: true);

    migrationBuilder.Sql(@"
        UPDATE ""Issue"" AS i
        SET ""SolutionDescription"" = SUBSTRING(ic.""Comment"", 11)
        FROM ""IssueComment"" AS ic
        WHERE ic.""IssueOrganizationId"" = i.""OrganizationId""
        AND ic.""IssueId"" = i.""Id""
        AND ic.""Comment"" LIKE 'Solution: %';");

    // Add back Description and restore its values from IssueComment
    migrationBuilder.AddColumn<string>(
        name: "Description",
        table: "Issue",
        type: "text",
        nullable: true);

    migrationBuilder.Sql(@"
        UPDATE ""Issue"" AS i
        SET ""Description"" = SUBSTRING(ic.""Comment"", 13)
        FROM ""IssueComment"" AS ic
        WHERE ic.""IssueOrganizationId"" = i.""OrganizationId""
        AND ic.""IssueId"" = i.""Id""
        AND ic.""Comment"" LIKE 'Description: %';");

    // Drop the IssueComment table
    migrationBuilder.DropTable(
        name: "IssueComment");

    // Drop foreign key for OrganizationId in Issue
    migrationBuilder.DropForeignKey(
        name: "FK_Issue_Organization_OrganizationId",
        table: "Issue");

    // Drop composite primary key and add back old primary key
    migrationBuilder.DropPrimaryKey(
        name: "PK_Issue",
        table: "Issue");

    migrationBuilder.AddPrimaryKey(
        name: "PK_Issue",
        table: "Issue",
        column: "Id");

    // Drop OrganizationId column from Issue
    migrationBuilder.DropColumn(
        name: "OrganizationId",
        table: "Issue");
}

    }
}
