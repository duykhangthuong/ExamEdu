using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services
{
    public class AdministratorService : IAdministratorService
    {
        private DataContext _dataContext;

        public AdministratorService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Get administrator by its email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Administrator> GetAdministratorByEmail(string email)
        {
            return await _dataContext.Administrators.Where(s => s.Email.ToLower().Equals(email.ToLower())).FirstOrDefaultAsync();
        }
    }
}