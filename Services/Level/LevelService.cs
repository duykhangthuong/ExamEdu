using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services
{
    public class LevelService : ILevelService
    {
        private readonly DataContext _db;
        public LevelService(DataContext db)
        {
            _db = db;
        }

        public async Task<Level> getLevelByID(int id)
        {
            return await _db.Levels.Where(l => l.LevelId == id).FirstOrDefaultAsync();
        }

        public bool IsLevelExist(int levelId)
        {
            return _db.Levels.Any(l => l.LevelId == levelId);
        }
    }
}