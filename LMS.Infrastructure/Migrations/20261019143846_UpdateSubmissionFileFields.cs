using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubmissionFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Submissions",
                newName: "StoredFileName");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Submissions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                table: "Submissions",
                newName: "FilePath");
        }
    }
}
