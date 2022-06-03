using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.DTO.TeacherDTO;
using ExamEdu.DB.Models;

namespace BackEnd.Services
{
    public interface IAcademicDepartmentService
    {
        Task<AcademicDepartment> GetAcademicDepartmentByEmail(string email);
        
        Task<IEnumerable<AcademicDepartment>> GetAcademicDeparment();
    }
}