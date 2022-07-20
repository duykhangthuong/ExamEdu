using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class Student
    {
         public int StudentId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DeactivatedAt { get; set; }

        // Many student - one role
        public int RoleId { get; set; }
        public Role Role { get; set; }

        // Many-Many exam
        public ICollection<Exam> Exams { get; set; }
        public ICollection<StudentExamInfo> StudentExamInfos { get; set; }

        public ICollection<StudentAnswer> StudentAnswers { get; set; }
        public ICollection<StudentFEAnswer> StudentFEAnswers { get; set; }

        // Many-Many ClassModule
        public ICollection<Class_Module_Student> Class_Module_Students { get; set; }
        public ICollection<ClassModule> ClassModules { get; set; }
        public ICollection<StudentCheating> StudentCheatings { get; set; }
    }
}