using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamEdu.Migrations
{
    public partial class AddExamPass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Exams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Room",
                table: "Exams",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "Room",
                table: "Exams");
        }
    }
}
