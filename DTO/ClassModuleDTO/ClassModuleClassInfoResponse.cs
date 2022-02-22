using ExamEdu.DTO.ClassDTO;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class ClassModuleWithClassInfoResponse
    {
        public string ClassModuleId { get; set; }
        public ClassNameResponse Class { get; set; }
    }
}