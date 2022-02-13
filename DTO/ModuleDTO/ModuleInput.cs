using System.ComponentModel.DataAnnotations;

namespace ExamEdu.DTO.ModuleDTO
{
    public class ModuleInput
    {
        [Required]
        public string ModuleCode { get; set; }
        [Required]
        public string ModuleName { get; set; }
    }
}