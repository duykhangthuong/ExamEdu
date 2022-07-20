using Microsoft.EntityFrameworkCore.Migrations;

namespace ExamEdu.Migrations
{
    public partial class adddbset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheating_CheatingType_CheatingTypeId",
                table: "StudentCheating");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheating_Exams_ExamId",
                table: "StudentCheating");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheating_Students_StudentId",
                table: "StudentCheating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCheating",
                table: "StudentCheating");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheatingType",
                table: "CheatingType");

            migrationBuilder.RenameTable(
                name: "StudentCheating",
                newName: "StudentCheatings");

            migrationBuilder.RenameTable(
                name: "CheatingType",
                newName: "CheatingTypes");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheating_StudentId",
                table: "StudentCheatings",
                newName: "IX_StudentCheatings_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheating_ExamId",
                table: "StudentCheatings",
                newName: "IX_StudentCheatings_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheating_CheatingTypeId",
                table: "StudentCheatings",
                newName: "IX_StudentCheatings_CheatingTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCheatings",
                table: "StudentCheatings",
                column: "StudentCheatingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheatingTypes",
                table: "CheatingTypes",
                column: "CheatingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheatings_CheatingTypes_CheatingTypeId",
                table: "StudentCheatings",
                column: "CheatingTypeId",
                principalTable: "CheatingTypes",
                principalColumn: "CheatingTypeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheatings_Exams_ExamId",
                table: "StudentCheatings",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheatings_Students_StudentId",
                table: "StudentCheatings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheatings_CheatingTypes_CheatingTypeId",
                table: "StudentCheatings");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheatings_Exams_ExamId",
                table: "StudentCheatings");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCheatings_Students_StudentId",
                table: "StudentCheatings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCheatings",
                table: "StudentCheatings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheatingTypes",
                table: "CheatingTypes");

            migrationBuilder.RenameTable(
                name: "StudentCheatings",
                newName: "StudentCheating");

            migrationBuilder.RenameTable(
                name: "CheatingTypes",
                newName: "CheatingType");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheatings_StudentId",
                table: "StudentCheating",
                newName: "IX_StudentCheating_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheatings_ExamId",
                table: "StudentCheating",
                newName: "IX_StudentCheating_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCheatings_CheatingTypeId",
                table: "StudentCheating",
                newName: "IX_StudentCheating_CheatingTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCheating",
                table: "StudentCheating",
                column: "StudentCheatingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheatingType",
                table: "CheatingType",
                column: "CheatingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheating_CheatingType_CheatingTypeId",
                table: "StudentCheating",
                column: "CheatingTypeId",
                principalTable: "CheatingType",
                principalColumn: "CheatingTypeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheating_Exams_ExamId",
                table: "StudentCheating",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "ExamId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCheating_Students_StudentId",
                table: "StudentCheating",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
