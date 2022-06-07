using System.Collections.Generic;
using BackEnd.DTO.TeacherDTO;
using examedu.DTO.StudentDTO;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.ModuleDTO;

namespace ExamEdu.DTO.ClassModuleDTO
{
    public class ClassModuleStudentResponse
    {
        public string ClassModuleId { get; set; }
        public ClassNameResponse Class { get; set; }
        public ModuleResponse Module { get; set; }
        public TeacherResponse Teacher { get; set; }
        public List<StudentResponse> Students { get; set; }
    }
}