using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.DTO.StudentDTO
{
    public class ModuleMarkDTO
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDate { get; set; }
        public float? Mark { get; set; }
        public string Comment { get; set; }
    }
}