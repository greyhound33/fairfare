using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairFare.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE UNIQUE INDEX \"UX_Participants_EventId_LowerName\" ON \"Participants\" (\"EventId\", lower(\"Name\"));");
            migrationBuilder.AddCheckConstraint(
                name: "CK_Participants_Name_NotBlank",
                table: "Participants",
                sql: "length(btrim(\"Name\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Events_Title_NotBlank",
                table: "Events",
                sql: "length(btrim(\"Title\")) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"UX_Participants_EventId_LowerName\";");
            migrationBuilder.DropCheckConstraint(
                name: "CK_Participants_Name_NotBlank",
                table: "Participants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Events_Title_NotBlank",
                table: "Events");
        }
    }
}
