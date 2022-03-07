using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamEdu.Migrations
{
    public partial class addIsApproveToQuestion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isHeadOfDepartment",
                table: "Teachers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isApproved",
                table: "Questions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "isApproved",
                table: "FEQuestions",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isHeadOfDepartment",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "isApproved",
                table: "Questions");

            migrationBuilder.AlterColumn<bool>(
                name: "isApproved",
                table: "FEQuestions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
