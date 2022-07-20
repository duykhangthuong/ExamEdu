using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;

namespace examedu.Services.Cheat
{
    public interface ICheatService
    {
        public Task<List<CheatingType>> GetCheatingTypeList();
        public Task<int> CreateStudentCheating(StudentCheating studentCheating);
    }
}