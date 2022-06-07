using System.Collections.Generic;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class TeacherStudentInput
    {
        public List<int> StudentIds { get; set; }
        public int TeacherId { get; set; }
    }
}