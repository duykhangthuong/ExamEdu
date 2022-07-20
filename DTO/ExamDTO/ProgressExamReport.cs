using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DTO.MarkDTO;

namespace ExamEdu.DTO.ExamDTO
{
    public class ProgressExamReport
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public List<StudentProgressResult> StudentMark { get; set; }
    }
}