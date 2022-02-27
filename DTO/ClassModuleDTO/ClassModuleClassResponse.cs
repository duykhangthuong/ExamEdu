using ExamEdu.DTO.ClassDTO;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class ClassModuleClassResponse
    {
        public string ClassModuleId { get; set; }
        public ClassNameResponse Class { get; set; }
    }
}