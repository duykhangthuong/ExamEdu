using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.DTO.ModuleDTO;

namespace ExamEdu.Services
{
    public interface IModuleService
    {
        Task<Tuple<int, IEnumerable<Module>>> getAllModuleStudentHaveExam(int studentId, PaginationParameter paginationParameter);
        Task<Module> getModuleByID(int id);
        Task<Module> getModuleByCode(string code);
        Task<Tuple<int, IEnumerable<Module>>> getModules(PaginationParameter paginationParameter);
        Task<int> InsertNewModule(ModuleInput moduleInput);
        Task<int> UpdateModule(ModuleInput moduleInput);
        Task<int> DeleteModule(int id);
    }
}