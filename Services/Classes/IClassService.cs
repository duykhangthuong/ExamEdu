using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace examedu.Services.Classes
{
    public interface IClassService
    {
        Task<Tuple<int, IEnumerable<Class>>> GetClasses(int teacherId, int moduleId, PaginationParameter paginationParameter);
    }
}