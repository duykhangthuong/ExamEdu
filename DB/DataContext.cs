using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamEdu.DB
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AcademicDepartment> AcademicDepartments { get; set; }
        public DbSet<AddQuestionRequest> AddQuestionRequests { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<FEAnswer> FEAnswers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassModule> ClassModules { get; set; }
        public DbSet<Class_Module_Student> Class_Module_Students { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<StudentExamInfo> StudentExamInfos { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<Exam_FEQuestion> Exam_FEQuestions { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<FEQuestion> FEQuestions { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<StudentFEAnswer> StudentFEAnswers { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<StudentCheating> StudentCheatings { get; set; }
        public DbSet<CheatingType> CheatingTypes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Auto increase Identity column
            modelBuilder.UseSerialColumns();

            // Role table to other account entities
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Administrators)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.AcademicDepartments)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Students)
                .WithOne(t => t.Role)
                .HasForeignKey(t => t.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Teachers)
                .WithOne(t => t.Role)
                .HasForeignKey(t => t.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            // connect AddQuestionRequest to question    
            modelBuilder.Entity<AddQuestionRequest>()
               .HasMany(r => r.Questions)
               .WithOne(a => a.AddQuestionRequest)
               .HasForeignKey(a => a.AddQuestionRequestId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect many-many relationship of class and module 
            modelBuilder.Entity<Class>()
                .HasMany(c => c.Modules)
                .WithMany(m => m.Classes)
                .UsingEntity<ClassModule>(
                    j => j
                        .HasOne(cm => cm.Module)
                        .WithMany(m => m.ClassModules)
                        .HasForeignKey(cm => cm.ModuleId),
                    j => j
                        .HasOne(cm => cm.Class)
                        .WithMany(c => c.ClassModules)
                        .HasForeignKey(cm => cm.ClassId),
                    j => j
                        .HasKey(cm => cm.ClassModuleId)
                );
            // set unique for classId and moduleId in classModule
            modelBuilder.Entity<ClassModule>()
                .HasIndex(cm => new { cm.ClassId, cm.ModuleId })
                .IsUnique();

            // Connect many-many relationship of classModule and student 
            modelBuilder.Entity<ClassModule>()
                .HasMany(c => c.Students)
                .WithMany(m => m.ClassModules)
                .UsingEntity<Class_Module_Student>(
                    j => j
                        .HasOne(cm => cm.Student)
                        .WithMany(m => m.Class_Module_Students)
                        .HasForeignKey(cm => cm.StudentId),
                    j => j
                        .HasOne(cm => cm.ClassModule)
                        .WithMany(c => c.Class_Module_Students)
                        .HasForeignKey(cm => cm.ClassModuleId),
                    j =>
                    {
                        j.HasKey(cm => new { cm.StudentId, cm.ClassModuleId });
                    }
                );

            // Connect many-many relationship of exam and question 
            modelBuilder.Entity<Exam>()
                .HasMany(c => c.Questions)
                .WithMany(m => m.Exams)
                .UsingEntity<ExamQuestion>(
                    j => j
                        .HasOne(cm => cm.Question)
                        .WithMany(m => m.ExamQuestions)
                        .HasForeignKey(cm => cm.QuestionId),
                    j => j
                        .HasOne(cm => cm.Exam)
                        .WithMany(c => c.ExamQuestions)
                        .HasForeignKey(cm => cm.ExamId),
                    j => j
                        .HasKey(cm => cm.ExamQuestionId)
                );
            // set unique for examId and questionId in examQuestion
            modelBuilder.Entity<ExamQuestion>()
                .HasIndex(eq => new { eq.ExamId, eq.QuestionId, eq.ExamCode })
                .IsUnique();

            // Connect many-many relationship of FE exam and question 
            modelBuilder.Entity<Exam>()
                .HasMany(c => c.FEQuestions)
                .WithMany(m => m.Exams)
                .UsingEntity<Exam_FEQuestion>(
                    j => j
                        .HasOne(cm => cm.FEQuestion)
                        .WithMany(m => m.Exam_FEQuestions)
                        .HasForeignKey(cm => cm.FEQuestionId),
                    j => j
                        .HasOne(cm => cm.Exam)
                        .WithMany(c => c.Exam_FEQuestions)
                        .HasForeignKey(cm => cm.ExamId),
                    j => j
                        .HasKey(cm => cm.ExamFEQuestionId)
                );
            // set unique for examId and questionId in examQuestion
            modelBuilder.Entity<Exam_FEQuestion>()
                .HasIndex(eq => new { eq.ExamId, eq.FEQuestionId, eq.ExamCode })
                .IsUnique();

            // connect exam question to student answer
            modelBuilder.Entity<ExamQuestion>()
               .HasMany(r => r.StudentAnswers)
               .WithOne(a => a.ExamQuestion)
               .HasForeignKey(a => a.ExamQuestionId)
               .OnDelete(DeleteBehavior.SetNull);

            // connect FE exam question to FE student answer
            modelBuilder.Entity<Exam_FEQuestion>()
               .HasMany(r => r.StudentFEAnswers)
               .WithOne(a => a.Exam_FEQuestion)
               .HasForeignKey(a => a.ExamFEQuestionId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect many-many relationship of exam and student 
            modelBuilder.Entity<Exam>()
                .HasMany(c => c.Students)
                .WithMany(m => m.Exams)
                .UsingEntity<StudentExamInfo>(
                    j => j
                        .HasOne(cm => cm.Student)
                        .WithMany(m => m.StudentExamInfos)
                        .HasForeignKey(cm => cm.StudentId),
                    j => j
                        .HasOne(cm => cm.Exam)
                        .WithMany(c => c.StudentExamInfos)
                        .HasForeignKey(cm => cm.ExamId),
                    j =>
                    {
                        j.HasKey(cm => new { cm.StudentId, cm.ExamId });
                    }
                );
            // set unique for examName in exam
            modelBuilder.Entity<Exam>()
                .HasIndex(eq => eq.ExamName)
                .IsUnique();

            // Connect module to exam
            modelBuilder.Entity<Module>()
               .HasMany(r => r.Exams)
               .WithOne(a => a.Module)
               .HasForeignKey(a => a.ModuleId)
               .OnDelete(DeleteBehavior.SetNull);
            // set unique for moduleCode in module
            modelBuilder.Entity<Module>()
                .HasIndex(eq => eq.ModuleCode)
                .IsUnique();
            // Connect level to question
            modelBuilder.Entity<Level>()
               .HasMany(r => r.Questions)
               .WithOne(a => a.Level)
               .HasForeignKey(a => a.LevelId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect level to question
            modelBuilder.Entity<Level>()
               .HasMany(r => r.FEQuestions)
               .WithOne(a => a.Level)
               .HasForeignKey(a => a.LevelId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect question type to question
            modelBuilder.Entity<QuestionType>()
               .HasMany(r => r.Questions)
               .WithOne(a => a.QuestionType)
               .HasForeignKey(a => a.QuestionTypeId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect question type to question
            modelBuilder.Entity<QuestionType>()
               .HasMany(r => r.FEQuestions)
               .WithOne(a => a.QuestionType)
               .HasForeignKey(a => a.QuestionTypeId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect module to Question
            modelBuilder.Entity<Module>()
               .HasMany(r => r.Questions)
               .WithOne(a => a.Module)
               .HasForeignKey(a => a.ModuleId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect module to Question
            modelBuilder.Entity<Module>()
               .HasMany(r => r.FEQuestions)
               .WithOne(a => a.Module)
               .HasForeignKey(a => a.ModuleId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect question to answer
            modelBuilder.Entity<Question>()
               .HasMany(r => r.Answers)
               .WithOne(a => a.Question)
               .HasForeignKey(a => a.QuestionId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect FE question to FE answer
            modelBuilder.Entity<FEQuestion>()
               .HasMany(r => r.FEAnswers)
               .WithOne(a => a.FEQuestion)
               .HasForeignKey(a => a.FEQuestionId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect student to studentAnswer
            modelBuilder.Entity<Student>()
               .HasMany(r => r.StudentAnswers)
               .WithOne(a => a.Student)
               .HasForeignKey(a => a.StudentId)
               .OnDelete(DeleteBehavior.SetNull);
            // Connect student to studentAnswer
            modelBuilder.Entity<Student>()
               .HasMany(r => r.StudentFEAnswers)
               .WithOne(a => a.Student)
               .HasForeignKey(a => a.StudentId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect teacher to exam as proctor
            // modelBuilder.Entity<Teacher>()
            //    .HasMany(r => r.ExamsToProc)
            //    .WithOne(a => a.Proctor)
            //    .HasForeignKey(a => a.ProctorId)
            //    .OnDelete(DeleteBehavior.SetNull);  
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Grader)
                .WithMany(t => t.ExamsToGrade)
                .HasForeignKey(t => t.GraderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Connect teacher to exam as grader
            modelBuilder.Entity<Teacher>()
               .HasMany(r => r.ExamsToGrade)
               .WithOne(a => a.Grader)
               .HasForeignKey(a => a.GraderId)
               .OnDelete(DeleteBehavior.NoAction);

            // Connect aca to exam as supervisor
            modelBuilder.Entity<AcademicDepartment>()
               .HasMany(r => r.Exams)
               .WithOne(a => a.Supervisor)
               .HasForeignKey(a => a.SupervisorId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect class module to exam
            modelBuilder.Entity<Teacher>()
               .HasMany(r => r.ClassModules)
               .WithOne(a => a.Teacher)
               .HasForeignKey(a => a.TeacherId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect AddQuestionRequest to teacher (Approver)
            modelBuilder.Entity<Teacher>()
               .HasMany(r => r.AddQuestionApproves)
               .WithOne(a => a.Approver)
               .HasForeignKey(a => a.ApproverId)
               .OnDelete(DeleteBehavior.SetNull);

            // Connect AddQuestionRequest to teacher (Requester)
            modelBuilder.Entity<Teacher>()
               .HasMany(r => r.AddQuestionRequests)
               .WithOne(a => a.Requester)
               .HasForeignKey(a => a.RequesterId)
               .OnDelete(DeleteBehavior.SetNull);
 
            // Connect StudentError to ErrorType
            modelBuilder.Entity<CheatingType>()
               .HasMany(r => r.StudentCheatings)
               .WithOne(a => a.CheatingType)
               .HasForeignKey(a => a.CheatingTypeId)
               .OnDelete(DeleteBehavior.SetNull);
            // Connect StudentCheating to Student
            modelBuilder.Entity<Student>()
                .HasMany<StudentCheating>(s => s.StudentCheatings)
                .WithOne(se => se.Student)
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
            // Connect StudentCheating to Exam
            modelBuilder.Entity<Exam>()
                .HasMany<StudentCheating>(s => s.StudentCheatings)
                .WithOne(se => se.Exam)
                .HasForeignKey(se => se.ExamId)
                .OnDelete(DeleteBehavior.SetNull);    
        }
    }
}