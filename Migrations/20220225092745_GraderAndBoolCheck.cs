using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamEdu.Migrations
{
    public partial class GraderAndBoolCheck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_ProctorId",
                table: "Exams");

            migrationBuilder.AddColumn<bool>(
                name: "NeedToGradeTextQuestion",
                table: "StudentExamInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GraderId",
                table: "Exams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_GraderId",
                table: "Exams",
                column: "GraderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_GraderId",
                table: "Exams",
                column: "GraderId",
                principalTable: "Teachers",
                principalColumn: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_ProctorId",
                table: "Exams",
                column: "ProctorId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_GraderId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_ProctorId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_GraderId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "NeedToGradeTextQuestion",
                table: "StudentExamInfos");

            migrationBuilder.DropColumn(
                name: "GraderId",
                table: "Exams");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_ProctorId",
                table: "Exams",
                column: "ProctorId",
                principalTable: "Teachers",
                principalColumn: "TeacherId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
