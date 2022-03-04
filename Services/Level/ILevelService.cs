using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;

namespace examedu.Services
{
    public interface ILevelService
    {
        public Task<Level> getLevelByID(int id);
        bool IsLevelExist(int levelId);
    }
}