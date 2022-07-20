using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.MarkDTO
{
    public class StudentProgressResult
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public float? Mark { get; set; }
    }
}