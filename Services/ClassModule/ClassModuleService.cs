
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using System.Linq;
using ExamEdu.DB;
using ExamEdu.DTO.PaginationDTO;
using Microsoft.EntityFrameworkCore;
using ExamEdu.Helper;
using ExamEdu.DTO.ModuleDTO;
using AutoMapper;



namespace ExamEdu.Services
{
    public class ClassModuleService : IClassModuleService
    {
        private readonly DataContext _db;
        public ClassModuleService(DataContext db)
        {
            this._db = db;
        }

        public async Task<Tuple<int, IEnumerable<ClassModule>>> GetClassModuleByTeacherId(int teacherId, PaginationParameter paginationParameter)
        {
            var queryResult = from cm in _db.ClassModules
                              join c in _db.Classes on cm.ClassId equals c.ClassId
                              join m in _db.Modules on cm.ModuleId equals m.ModuleId
                              where cm.TeacherId == teacherId
                              select new ClassModule
                              {
                                  ClassModuleId = cm.ClassModuleId,
                                  Class = new Class
                                  {
                                      ClassId = c.ClassId,
                                      ClassName = c.ClassName
                                  },
                                  Module = new Module
                                  {
                                      ModuleId = m.ModuleId,
                                      ModuleCode = m.ModuleCode,
                                      ModuleName = m.ModuleName
                                  }
                              };
            var classModules = await queryResult.ToListAsync();

            var totalCount = classModules.Count;

            return new Tuple<int, IEnumerable<ClassModule>>(totalCount, classModules.GetPage(paginationParameter));
        }

        public async Task<ClassModule> GetClassModuleInfo(int classModuleId)
        {
            //Get class module info from classModuleId
            var queryResult = from cm in _db.ClassModules
                              join c in _db.Classes on cm.ClassId equals c.ClassId
                              join m in _db.Modules on cm.ModuleId equals m.ModuleId
                              where cm.ClassModuleId == classModuleId
                              select new ClassModule
                              {
                                  ClassModuleId = cm.ClassModuleId,
                                  Class = new Class
                                  {
                                      ClassId = c.ClassId,
                                      ClassName = c.ClassName
                                  },
                                  Module = new Module
                                  {
                                      ModuleId = m.ModuleId,
                                      ModuleCode = m.ModuleCode,
                                      ModuleName = m.ModuleName
                                  }
                              };
            var classModule = await queryResult.FirstOrDefaultAsync();

            return classModule;
        }
    }
}
