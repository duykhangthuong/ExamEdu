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
    public class TeacherService : ITeacherService
    {
        private readonly DataContext _dataContext;
        public TeacherService(DataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        /// <summary>
        /// Get teacher by its email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Teacher> GetTeacherByEmail(string email)
        {
            return await _dataContext.Teachers.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
        }

        public async Task<bool> IsTeacherExist(int id)
        {
            return await _dataContext.Teachers.Where(s => s.TeacherId == id && s.DeactivatedAt == null).AnyAsync();
        }

        /// <summary>
        /// Get ID and name of all teacher in database
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Teacher>> GetAllTeacherIdAndName()
        {
            return await _dataContext.Teachers.Where(t => t.DeactivatedAt == null).Select(t => new Teacher { TeacherId = t.TeacherId, Fullname = t.Fullname }).ToListAsync();
        }

        public async Task<bool> IsHeadOfDepartment(int teacherId)
        {
            return await _dataContext.Teachers.Where(t => t.TeacherId == teacherId && t.DeactivatedAt == null).Select(t => t.isHeadOfDepartment).FirstOrDefaultAsync();
        }
    }
}