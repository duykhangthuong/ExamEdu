using System.Collections.Generic;

namespace BackEnd.DTO.ClassModuleDTO
{
    public class ModuleTeacherStudentInput
    {
        public ICollection<int> StudentIds { get; set; }
        public int TeacherId { get; set; }
        public int ModuleId { get; set; }
    }
}