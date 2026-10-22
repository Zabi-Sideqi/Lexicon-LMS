using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherCourseRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Courses_CourseId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "ApplicationUserCourse",
                columns: table => new
                {
                    TeachersId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeachingCoursesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserCourse", x => new { x.TeachersId, x.TeachingCoursesId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserCourse_AspNetUsers_TeachersId",
                        column: x => x.TeachersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserCourse_Courses_TeachingCoursesId",
                        column: x => x.TeachingCoursesId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserCourse_TeachingCoursesId",
                table: "ApplicationUserCourse",
                column: "TeachingCoursesId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Courses_CourseId",
                table: "AspNetUsers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Courses_CourseId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ApplicationUserCourse");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Courses_CourseId",
                table: "AspNetUsers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
