using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.EntityFrameworkCore;

namespace ExamEdu.Services
{
    public class ModuleService :IModuleService
    {
        private readonly DataContext _db;
        public ModuleService(DataContext db)
        {
            _db = db;
        }
        public async Task<Tuple<int, IEnumerable<Module>>> GetAllModuleStudentHaveExam(int studentId,PaginationParameter paginationParameter)
        {
            //Select all the exam student have attend
            var allExamOfStudent = await _db.StudentExamInfos.Where(e => e.StudentId == studentId).Select(e => e.ExamId).ToListAsync();
            if (allExamOfStudent.Count() == 0)
            {
                return null;
            }
            //Select all module Id in the exam student have attend by joining Exam table and module table
            var moduleList =await _db.Exams.Join(_db.Modules,
                                             e => e.ModuleId, 
                                             m => m.ModuleId,
                                            (e, m) => new { 
                                                ExamId = e.ExamId, 
                                                ModuleId = m.ModuleId, 
                                                ModuleName = m.ModuleName 
                                                })
                                        .Where(e => allExamOfStudent.Contains(e.ExamId))
                                        .Select(m => new Module { 
                                                    ModuleId = m.ModuleId, 
                                                    ModuleName = m.ModuleName })
                                        .Distinct().ToListAsync();
            return Tuple.Create(moduleList.Count, moduleList.GetPage(paginationParameter));

        }

        public async Task<Module> GetModuleByID(int id)
        {
            return await _db.Modules.Where(m => m.ModuleId == id).FirstOrDefaultAsync();
        }
    }
}