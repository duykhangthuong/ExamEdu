using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class StudentCheating
    {
        public int StudentCheatingId { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public DateTime Time { get; set; }
        public string Comment { get; set; }
        public bool IsComfirmed { get; set; }=false;
        public int CheatingTypeId { get; set; }
        public CheatingType CheatingType { get; set; }
    }
}