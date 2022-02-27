using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.StudentDTO;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services
{
    public class StudentService : IStudentService
    {
        private readonly DataContext _dataContext;

        public StudentService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        /// <summary>
        /// get list of moduleMark of a module
        /// </summary>
        /// <param name="studentID"></param>
        /// <param name="moduleID"></param>
        /// <returns>null if moduleID no exits, empty list if no exam available</returns>
        public async Task<List<ModuleMarkDTO>> getModuleMark(int studentID, int moduleID)
        {
            List<ModuleMarkDTO> listToRetrun = new List<ModuleMarkDTO>();
            Module moduleInfor = await _dataContext.Modules.Where(m => m.ModuleId == moduleID).FirstOrDefaultAsync();
            if (moduleInfor == null)
            {
                return null; //check if module exist
            }
            if (await _dataContext.Students.Where(s => s.StudentId == studentID).FirstOrDefaultAsync() == null)
            {
                return null;
            }

            List<StudentExamInfo> studentExamInforList = await _dataContext.StudentExamInfos.Where(s => s.StudentId == studentID).ToListAsync();
            if (studentExamInforList == null)
            {
                return listToRetrun;
            }

            List<Exam> examList = new List<Exam>();

            foreach (var exam in studentExamInforList)
            {
                var temp = await _dataContext.Exams.Where(e => e.ModuleId == moduleID && e.ExamId == exam.ExamId).FirstOrDefaultAsync();
                if (temp != null)
                {
                    examList.Add(temp);
                }

            }

            if (examList.Count == 0)
            {
                return listToRetrun;
            }
            if (examList.Count() == 0)
            {
                return listToRetrun;
            }
            foreach (var exam in examList)
            {
                ModuleMarkDTO moduleMarkInfor = new ModuleMarkDTO();
                moduleMarkInfor.ExamDate = exam.ExamDay;
                moduleMarkInfor.ExamName = exam.ExamName;
                moduleMarkInfor.ModuleName = moduleInfor.ModuleName;
                moduleMarkInfor.ModuleID = moduleInfor.ModuleId;
                foreach (var studetExamInfor in studentExamInforList)
                {
                    if (studetExamInfor.ExamId == exam.ExamId)
                    {
                        moduleMarkInfor.Mark = studetExamInfor.Mark;
                        moduleMarkInfor.Comment = studetExamInfor.Comment;
                    }
                }
                listToRetrun.Add(moduleMarkInfor);
            }
            return listToRetrun;
        }
        public bool CheckStudentExist(int id)
        {
            return _dataContext.Students.Any(t => t.StudentId == id &&
           t.DeactivatedAt == null);
        }

        /// <summary>
        /// Get a student by its email
        /// </summary>
        /// <param name="email">student email</param>
        /// <returns></returns>
        public async Task<Student> GetStudentByEmail(string email)
        {
            return await _dataContext.Students.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
        }

        public async Task<Tuple<int, IEnumerable<Student>>> GetStudents(int teacherId, int moduleId, PaginationParameter paginationParameter)
        {
            var queryResult = from student in _dataContext.Students
                              join cms in _dataContext.Class_Module_Students on student.StudentId equals cms.StudentId
                              join cm in _dataContext.ClassModules on cms.ClassModuleId equals cm.ClassModuleId
                              where cm.ModuleId == moduleId && cm.TeacherId == teacherId && student.DeactivatedAt == null
                              select new Student
                              {
                                  StudentId = student.StudentId,
                                  Fullname = student.Fullname
                              };
            var students = await queryResult.ToListAsync();
            return Tuple.Create(students.Count, students.GetPage(paginationParameter));
        }

        /// <summary>
        /// Get students of a class by classId and pagination
        /// </summary>
        /// <param name="classModuleId">The classmodule's id</param>
        /// <param name="paginationParameter">Pagination parameters</param>
        /// <returns>
        /// 
        /// </returns>
        public async Task<Tuple<int, IEnumerable<Student>>> GetStudents(int classModuleId, PaginationParameter paginationParameter)
        {
            var queryResult = from student in _dataContext.Students
                              join cms in _dataContext.Class_Module_Students on student.StudentId equals cms.StudentId
                              join cm in _dataContext.ClassModules on cms.ClassModuleId equals cm.ClassModuleId
                              where cm.ClassModuleId == classModuleId && student.DeactivatedAt == null
                              select new Student
                              {
                                  StudentId = student.StudentId,
                                  Fullname = student.Fullname
                              };
            var students = await queryResult.ToListAsync();

            return Tuple.Create(students.Count, students.GetPage(paginationParameter));
        }
    }
}