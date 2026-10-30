
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "RecipientId",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: true);

            // Bevara befintliga notiser genom att kopiera
            // studentens ID till den nya generella mottagaren.
            migrationBuilder.Sql("""
                UPDATE [Notifications]
                SET [RecipientId] = [StudentId]
                WHERE [StudentId] IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
