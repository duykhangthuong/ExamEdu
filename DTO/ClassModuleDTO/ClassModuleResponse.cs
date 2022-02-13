using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.ModuleDTO;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class ClassModuleResponse
    {
        public string ClassModuleId { get; set; }
        public ClassNameResponse Class { get; set; }
        public ModuleResponse Module { get; set; }
    }
}