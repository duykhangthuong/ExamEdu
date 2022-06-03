using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.DTO.TeacherDTO;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services
{
    public class AcademicDepartmentService : IAcademicDepartmentService
    {
        private readonly DataContext _dataContext;
        public AcademicDepartmentService(DataContext dataContext)
        {
            this._dataContext=dataContext;
        }

        public async Task<IEnumerable<AcademicDepartment>> GetAcademicDeparment()
        {
            var result = await _dataContext.AcademicDepartments.ToListAsync();
            return result;
        }

        public async Task<AcademicDepartment> GetAcademicDepartmentByEmail(string email)
        {
            return await _dataContext.AcademicDepartments.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
        }
    }
}