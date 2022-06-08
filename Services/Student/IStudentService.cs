using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.StudentDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace examedu.Services
{
    public interface IStudentService
    {
        Task<List<ModuleMarkDTO>> getModuleMark(int studentID, int moduleID);
        bool CheckStudentExist(int id);
        Task<Student> GetStudentByEmail(string email);
        Task<Tuple<int, IEnumerable<Student>>> GetStudents(int teacherId, int moduleId, PaginationParameter paginationParameter);
        Task<Tuple<int, IEnumerable<Student>>> GetAllStudents(int moduleId, PaginationParameter paginationParameter);
        Task<Tuple<int, IEnumerable<Student>>> GetStudents(int classModuleId, PaginationParameter paginationParameter);
        Task<Tuple<int, IEnumerable<Student>>> GetStudentsNotInClassModule(int classId,int moduleId, PaginationParameter paginationParameter);
    }
}