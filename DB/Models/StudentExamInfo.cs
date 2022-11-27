using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class StudentExamInfo
    {
        
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        
        public int StudentId { get; set; }
        public Student Student { get; set; }
        
        public float? Mark { get; set; }
        public DateTime? FinishAt { get; set; }
        public DateTime? MaxFinishTime { get; set; }
        public string Comment { get; set; }

        public bool NeedToGradeTextQuestion { get; set; }
    }
}