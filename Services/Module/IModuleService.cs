using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace ExamEdu.Services
{
    public interface IModuleService
    {
        Task<Tuple<int, IEnumerable<Module>>> GetAllModuleStudentHaveExam(int studentId,PaginationParameter paginationParameter);
        Task<Module> GetModuleByID(int id);
    }
}