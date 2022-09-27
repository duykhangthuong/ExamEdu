using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.DTO.Email;

namespace BackEnd.Helper.Email
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(EmailContent emailContent);
        public bool IsValidEmail(string email);
    }
}