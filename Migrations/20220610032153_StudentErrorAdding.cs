using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ExamEdu.Migrations
{
    public partial class StudentErrorAdding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorType",
                columns: table => new
                {
                    ErrorTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ErrorTypeName = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorType", x => x.ErrorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StudentError",
                columns: table => new
                {
                    StudentErrorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    IsComfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentError", x => x.StudentErrorId);
                    table.ForeignKey(
                        name: "FK_StudentError_ErrorType_ErrorTypeId",
                        column: x => x.ErrorTypeId,
                        principalTable: "ErrorType",
                        principalColumn: "ErrorTypeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentError_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentError_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentError_ErrorTypeId",
                table: "StudentError",
                column: "ErrorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentError_ExamId",
                table: "StudentError",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentError_StudentId",
                table: "StudentError",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentError");

            migrationBuilder.DropTable(
                name: "ErrorType");
        }
    }
}
