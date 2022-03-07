using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;

namespace BackEnd.Services
{
    public interface ITeacherService
    {
        Task<Teacher> GetTeacherByEmail(string email);
        Task<bool> IsTeacherExist(int id);
        Task<IEnumerable<Teacher>> GetAllTeacherIdAndName();
        Task<bool> IsHeadOfDepartment(int teacherId);
    }
}