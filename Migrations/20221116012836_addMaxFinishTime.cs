using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamEdu.Migrations
{
    public partial class addMaxFinishTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MaxFinishTime",
                table: "StudentExamInfos",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxFinishTime",
                table: "StudentExamInfos");
        }
    }
}
