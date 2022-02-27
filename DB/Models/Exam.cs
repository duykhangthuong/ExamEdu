using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string Description { get; set; }
        public DateTime ExamDay { get; set; }
        public int DurationInMinute { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool isFinalExam { get; set; }
        public bool IsCancelled { get; set; } = false;
        public string Password { get; set; }
        public string Room { get; set; }

        // 1 Teacher - many exam
        public int ProctorId { get; set; }
        public Teacher Proctor { get; set; }

        // 1 Teacher - many exam
        public int? GraderId { get; set; }
        public Teacher Grader { get; set; }

        //1 aca - many exam (as a supervisor)
        public int SupervisorId { get; set; }
        public AcademicDepartment Supervisor { get; set; }

        // 1  Module - many exam
        public int ModuleId { get; set; }
        public Module Module { get; set; }

        // Many-Many Student
        public ICollection<StudentExamInfo> StudentExamInfos { get; set; }
        public ICollection<Student> Students { get; set; }

        // Many-Many Question
        public ICollection<Question> Questions { get; set; }
        public ICollection<ExamQuestion> ExamQuestions { get; set; }

        public ICollection<FEQuestion> FEQuestions { get; set; }
        public ICollection<Exam_FEQuestion> Exam_FEQuestions { get; set; }
    }
}