using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace ExamEdu.Services
{
    public interface IClassModuleService
    {
        public Task<Tuple<int, IEnumerable<ClassModule>>> GetClassModuleByTeacherId(int teacherId, PaginationParameter paginationParameter);
        public Task<ClassModule> GetClassModuleInfo(int classModuleId);
        public Task<Tuple<int, IEnumerable<ClassModule>>> GetClassModules(int teacherId, int moduleId, PaginationParameter paginationParameter);
        Task<List<ClassModule>> GetClassModuleList(int classId);
    }
}