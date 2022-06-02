using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using Microsoft.EntityFrameworkCore;
using ExamEdu.Helper;
using ExamEdu.DTO.ClassDTO;

namespace examedu.Services.Classes
{
    public class ClassService : IClassService
    {
        private readonly DataContext _db;

        public ClassService(DataContext db)
        {
            _db = db;
        }

        public async Task<Class> GetClassBasicInforById(int classId)
        {
            return await _db.Classes.Where(c => c.ClassId == classId).FirstOrDefaultAsync();
        }



        /// <summary>
        /// Get all classes that belongs to a teacherID and a moduleId combination
        /// </summary>
        /// <param name="teacherId">Teacher's id</param>
        /// <param name="moduleId">Modules's id</param>
        /// <param name="paginationParameter">Pagination parameters</param>
        /// <returns>
        /// A list of classes that belongs to a combination  of teacherID and moduleId
        /// </returns>
        public async Task<Tuple<int, IEnumerable<Class>>> GetClasses(int teacherId, int moduleId, PaginationParameter paginationParameter)
        {
            //Get all teacher classes by teacherId and pagination
            var queryResult = from c in _db.Classes
                              join cm in _db.ClassModules on c.ClassId equals cm.ClassId
                              join m in _db.Modules on cm.ModuleId equals m.ModuleId
                              where cm.TeacherId == teacherId && m.ModuleId == moduleId && c.DeactivatedAt == null
                              select new Class
                              {
                                  ClassId = c.ClassId,
                                  ClassName = c.ClassName
                              };

            var classes = await queryResult.ToListAsync();

            return Tuple.Create(classes.Count, classes.GetPage(paginationParameter));
        }

        /// <summary>
        /// Get all classes in database (by academic department)
        /// </summary>
        public async Task<Tuple<int, IEnumerable<Class>>> GetAllClasses(PaginationParameter paginationParameter)
        {
            // Get all class in database
            var classes = await _db.Classes.Where(c => c.DeactivatedAt == null).ToListAsync();
            return Tuple.Create(classes.Count, classes.GetPage(paginationParameter));
        }

        public async Task<bool> IsClassNameExist(string className)
        {
            return await _db.Classes.AnyAsync(c => c.ClassName.Equals(className) && c.DeactivatedAt == null);
        }

        public async Task<int> CreateNewClass(Class classInput)
        {
            int rowInserted = 0;
            try
            {
                _db.Classes.Add(classInput);
                rowInserted = await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                rowInserted = -1;
            }
            return rowInserted;
        }

        public async Task<bool> IsClassExist(int classId)
        {
            return await _db.Classes.Where(s => s.ClassId == classId && s.DeactivatedAt == null).AnyAsync();
        }

        public async Task<int> UpdateClassBasicInfor(Class classUpdated)
        {
            _db.Classes.Attach(classUpdated);
            var entry = _db.Entry(classUpdated);

            entry.Property(e => e.ClassName).IsModified = true;
            entry.Property(e => e.StartDay).IsModified = true;
            entry.Property(e => e.EndDay).IsModified = true;

            int result = await _db.SaveChangesAsync();

            return result;
        }
    }
}