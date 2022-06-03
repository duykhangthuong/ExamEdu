using ExamEdu.DTO.ModuleDTO;
using System;

namespace ExamEdu.DTO.ExamDTO
{
    public class GetAllExam
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDay { get; set; }
        public ModuleResponse Module { get; set; }
    }
}
