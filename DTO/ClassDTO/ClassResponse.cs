
using ExamEdu.DTO.ClassModuleDTO;
using System;
using System.Collections.Generic;

namespace ExamEdu.DTO.ClassDTO
{
    public class ClassResponse
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }

        public List<ClassModuleResponse2> ClassModules { get; set; }
    }
}
