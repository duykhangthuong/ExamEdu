using System;
using System.Collections.Generic;
using ExamEdu.DTO.ClassModuleDTO;

namespace ExamEdu.DTO.ModuleDTO
{
    public class ModuleTeacherResponse
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public List<ClassModuleWithClassInfoResponse> ClassModules { get; set; }
    }
}
