using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.ExamDTO
{
    public class ExamScheduleResponse
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string Description { get; set; }
        public string ModuleCode { get; set; }
        public DateTime ExamDay { get; set; }
        public string Password { get; set; }
        public int DurationInMinute { get; set; }
    }
}