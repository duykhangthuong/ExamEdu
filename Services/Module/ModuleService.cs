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

        public async Task<Tuple<int, IEnumerable<ModuleResponse>>> getAllModuleStudentHaveLearn(int studentId, PaginationParameter paginationParameter)
        {
            var moduleList = await _db.Class_Module_Students.Join(_db.ClassModules,
                                                                cms => cms.ClassModuleId,
                                                                cm => cm.ClassModuleId,
                                                                (cms, cm) => new
                                                                {
                                                                    StudentId = cms.StudentId,
                                                                    ModuleId = cm.ModuleId,
                                                                    TeacherId = cm.TeacherId,
                                                                })
                                                                .Where(c => c.StudentId == studentId)
                                                                .Join(_db.Modules,
                                                                c => c.ModuleId,
                                                                m => m.ModuleId,
                                                                (c, m) => new
                                                                {
                                                                    ModuleId = m.ModuleId,
                                                                    ModuleCode = m.ModuleCode,
                                                                    ModuleName = m.ModuleName,
                                                                    TeacherId = c.TeacherId
                                                                })
                                                                .Join(_db.Teachers,
                                                                 c => c.TeacherId,
                                                                 t => t.TeacherId,
                                                                 (c, t) => new ModuleResponse
                                                                 {
                                                                     ModuleId = c.ModuleId,
                                                                     ModuleCode = c.ModuleCode,
                                                                     ModuleName = c.ModuleName,
                                                                     TeacherEmail = t.Email
                                                                 })
                                                                    .ToListAsync();


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
                         where m.ClassModules.Any(cm => cm.TeacherId == teacherId)
                         select new Module
                         {
                             ModuleId = m.ModuleId,
                             ModuleCode = m.ModuleCode,
                             ModuleName = m.ModuleName,
                             //Select ClassModules along with classes of each classmodule
                             ClassModules = m.ClassModules.Select(cm => new ClassModule
                             {
                                 ClassModuleId = cm.ClassModuleId,
                                 Class = cm.Class,
                             }).ToList()

                         };




            var modules = await result.ToListAsync();

            return Tuple.Create(modules.Count, modules.GetPage(paginationParameter));
            // return Tuple.Create(result.Count, result.GetPage(paginationParameter));
        }

        /// <summary>
        /// Check whether a module exist
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public bool IsModuleExist(int moduleId)
        {
            return _db.Modules.Any(m => m.ModuleId == moduleId);
        }

        /// <summary>
        /// Get all of the module ids of modules teacher teaches
        /// </summary>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<int>> GetAllModuleIdByTeacherId(int teacherId)
        {
            return await _db.ClassModules.Where(t => t.TeacherId == teacherId).Select(m => m.ModuleId).ToListAsync();
        }

        public async Task<Tuple<int, IEnumerable<Module>>> getModulesByTeacherId(int teacherId, PaginationParameter paginationParameter)
        {
            var queryResult = from m in _db.Modules
                              join cm in _db.ClassModules on m.ModuleId equals cm.ModuleId
                              where cm.TeacherId == teacherId
                              select new Module
                              {
                                  ModuleId = m.ModuleId,
                                  ModuleCode = m.ModuleCode,
                                  ModuleName = m.ModuleName,
                              };
            var modules = await queryResult.ToListAsync();
            return Tuple.Create(modules.Count, modules.GetPage(paginationParameter));
        }
    }
}