using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.ExamDTO
{
    public class ExamProctorResponse
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string Room { get; set; }
        public string Password { get; set; }
        public DateTime ExamDay { get; set; }
        public int DurationInMinute { get; set; }
        public bool isFinalExam { get; set; }
        public bool IsCancelled { get; set; }
        public string SupervisorEmail { get; set; }
    }
}