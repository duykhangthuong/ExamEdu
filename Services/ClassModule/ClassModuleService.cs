
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

        public async Task<List<ClassModule>> GetClassModuleList(int classId)
        {

            //var queryResult = await _db.ClassModules.Where(cm => cm.ClassId == classId).ToListAsync();

            var result = from cm in _db.ClassModules
                         where cm.ClassId == classId
                         join m in _db.Modules on cm.ModuleId equals m.ModuleId
                         join t in _db.Teachers on cm.TeacherId equals t.TeacherId
                         select new ClassModule
                         {
                             ClassModuleId = cm.ClassModuleId,
                             Module = new Module
                             {
                                 ModuleCode = m.ModuleCode,
                                 ModuleName = m.ModuleName
                             },
                             Teacher = new Teacher
                             {
                                 Fullname = t.Fullname,
                             }

                         };

            var queryResult = await result.ToListAsync();
            return queryResult;
        }

        public async Task<Tuple<int, IEnumerable<ClassModule>>> GetClassModules(int teacherId, int moduleId, PaginationParameter paginationParameter)
        {
            var queryResult = from cm in _db.ClassModules
                              join c in _db.Classes on cm.ClassId equals c.ClassId
                              where cm.TeacherId == teacherId && cm.ModuleId == moduleId
                              select new ClassModule
                              {
                                  ClassModuleId = cm.ClassModuleId,
                                  Class = new Class
                                  {
                                      ClassId = c.ClassId,
                                      ClassName = c.ClassName
                                  }
                              };
            var classModules = await queryResult.ToListAsync();

            return Tuple.Create(classModules.Count, classModules.GetPage(paginationParameter));
        }
    }
}
