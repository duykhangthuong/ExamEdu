using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services.Cheat
{
    public class CheatService : ICheatService
    {
        private readonly DataContext _dataContext;
        public CheatService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //get CheatingTypeList
        public async Task<List<CheatingType>> GetCheatingTypeList()
        {
            return await _dataContext.CheatingTypes.ToListAsync();
        }

        //create student cheating
        public async Task<int> CreateStudentCheating(StudentCheating studentCheating)
        {
            await _dataContext.StudentCheatings.AddAsync(studentCheating);
            return await _dataContext.SaveChangesAsync();
        }
    }
}