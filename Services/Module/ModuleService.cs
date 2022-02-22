using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.EntityFrameworkCore;
using ExamEdu.DTO.ModuleDTO;
using AutoMapper;

namespace ExamEdu.Services
{
    public class ModuleService : IModuleService
    {
        private readonly DataContext _db;
        private readonly IMapper _mapper;
        public ModuleService(DataContext db, IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<Tuple<int, IEnumerable<Module>>> getAllModuleStudentHaveExam(int studentId, PaginationParameter paginationParameter)
        {
            //Select all the exam student have attend
            var allExamOfStudent = await _db.StudentExamInfos.Where(e => e.StudentId == studentId).Select(e => e.ExamId).ToListAsync();
            if (allExamOfStudent.Count() == 0)
            {
                return null;
            }
            //Select all module Id in the exam student have attend by joining Exam table and module table
            var moduleList = await _db.Exams.Join(_db.Modules,
                                             e => e.ModuleId,
                                             m => m.ModuleId,
                                            (e, m) => new
                                            {
                                                ExamId = e.ExamId,
                                                ModuleId = m.ModuleId,
                                                ModuleName = m.ModuleName
                                            })
                                        .Where(e => allExamOfStudent.Contains(e.ExamId))
                                        .Select(m => new Module
                                        {
                                            ModuleId = m.ModuleId,
                                            ModuleName = m.ModuleName
                                        })
                                        .Distinct().ToListAsync();
            return Tuple.Create(moduleList.Count, moduleList.GetPage(paginationParameter));

        }

        public async Task<Module> getModuleByCode(string code)
        {
            return await _db.Modules.Where(m => m.ModuleCode.ToUpper() == code.ToUpper()).FirstOrDefaultAsync();
        }

        public async Task<Module> getModuleByID(int id)
        {
            return await _db.Modules.Where(m => m.ModuleId == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get modules by pagination
        /// </summary>
        /// <param name="paginationParameter">Parameter for pagination</param>
        /// <returns>A tuple containing the total amount of records and a list of paginated modules</returns>
        public async Task<Tuple<int, IEnumerable<Module>>> getModules(PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");
            searchName = ConvertToUnsign(searchName);

            var moduleList = await _db.Modules.Where(m => m.ModuleName.ToUpper().Contains(searchName.ToUpper()) || m.ModuleCode.ToUpper().Contains(searchName.ToUpper())).OrderBy(m => m.ModuleId).ToListAsync();
            return Tuple.Create(moduleList.Count, moduleList.GetPage(paginationParameter));
        }


        /// <summary>
        /// Insert a new module into the database
        /// </summary>
        /// <param name="moduleInput">Detail of the module: moduleCode, moduleName</param>
        /// <returns>An integer indicating if the insertion was successful. 1: success / 0: fail</returns>
        public async Task<int> InsertNewModule(ModuleInput moduleInput)
        {
            int result = 0;
            var newModule = _mapper.Map<Module>(moduleInput);   //Map DTO to Model
            newModule.CreatedAt = DateTime.Now;

            _db.Modules.Add(newModule);
            result = await _db.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// Update an existing module with new information
        /// </summary>
        /// <param name="moduleInput">Detail of the module: moduleCode, moduleName</param>
        /// <returns>An integer indicating if the update action waas successful. 1: sucess / 0: fail</returns>
        public async Task<int> UpdateModule(ModuleInput moduleInput)
        {
            var module = await getModuleByCode(moduleInput.ModuleCode);

            module.ModuleName = moduleInput.ModuleName;

            return await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Delete a module by id
        /// </summary>
        /// <param name="id">The id of the module</param>
        /// <returns>An integer indicating if the delete action was sucessful. 1: success / 0: fail</returns>
        public async Task<int> DeleteModule(int id)
        {
            _db.Modules.Remove(await getModuleByID(id));
            return await _db.SaveChangesAsync();
        }

        private string ConvertToUnsign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty)
                        .Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public async Task<Tuple<int, IEnumerable<Module>>> getModulesWithClassModule(PaginationParameter paginationParameter, int teacherId)
        {
            var result = from m in _db.Modules
                         join cm in _db.ClassModules on m.ModuleId equals cm.ModuleId
                         join c in _db.Classes on cm.ClassId equals c.ClassId
                         select new Module
                         {
                             ModuleId = m.ModuleId,
                             ModuleCode = m.ModuleCode,
                             ModuleName = m.ModuleName,
                             ClassModules = new List<ClassModule> { new ClassModule {ClassModuleId = cm.ClassModuleId, Class = c } }
                         };
            var modules = await result.ToListAsync();

            return Tuple.Create(modules.Count, modules.GetPage(paginationParameter));
        }
    }
}