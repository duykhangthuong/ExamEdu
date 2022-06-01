using ExamEdu.DB.Models;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class ClassModuleResponse2
    {
        //public int ClassModuleId { get; set; }

        //public Module Module { get; set; }

        //public Teacher Teacher { get; set; }

        public int ClassModuleId { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string TeacherName { get; set; }

    }
}
