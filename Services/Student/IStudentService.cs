using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.StudentDTO;
using ExamEdu.DB.Models;

namespace examedu.Services
{
    public interface IStudentService
    {
        Task<List<ModuleMarkDTO>> GetModuleMark(int studentID, int moduleID);
        bool CheckStudentExist(int id);
        Task<Student> GetStudentByEmail(string email);
    }
}