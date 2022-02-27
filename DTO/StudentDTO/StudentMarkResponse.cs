using System;

namespace examedu.DTO.StudentDTO
{
    public class StudentMarkResponse
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime? FinishedAt { get; set; }
        public float? Mark { get; set; }
        public bool NeedToGradeTextQuestion { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDay { get; set; }

    }
}