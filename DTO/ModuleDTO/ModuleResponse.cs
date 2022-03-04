using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.ModuleDTO
{
    public class ModuleResponse
    {
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string TeacherEmail { get; set; }
    }
}