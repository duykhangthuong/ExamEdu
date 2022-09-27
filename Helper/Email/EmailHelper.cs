using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using BackEnd.DTO.Email;
using Microsoft.AspNetCore.Hosting;

namespace BackEnd.Helper.Email
{
    public class EmailHelper : IEmailHelper
    {
        private readonly EmailConfig _emailConfig;
        private readonly IWebHostEnvironment _env;
        public EmailHelper(EmailConfig emailConfig, IWebHostEnvironment env)
        {
            this._emailConfig = emailConfig;
            this._env = env;
        }

        /// <summary>
        /// Send an auto email to selected mail address
        /// </summary>
        /// <param name="emailContent">Class include receiver information</param>
        /// <returns></returns>
        public async Task SendEmailAsync(EmailContent emailContent)
        {
            try
            {
                // Mail service config
                MailMessage mail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

                // Ready mail template
                string filePath = "beefree.html";
                StreamReader str = new StreamReader(filePath);
                string mailTemplate = await str.ReadToEndAsync();
                str.Close();

                mailTemplate = mailTemplate.Replace("[@mailcontent@]", emailContent.Body.Trim());

                // Mail content config
                mail.From = new MailAddress(_emailConfig.MailAddress, "ExamEdu");
                mail.To.Add(emailContent.ToEmail);
                mail.Subject = emailContent.Subject;
                mail.IsBodyHtml = true;
                mail.Body = mailTemplate;

                // Port & Login to Mail account
                smtpClient.Port = _emailConfig.MailPort;
                smtpClient.Credentials = new System.Net.NetworkCredential(_emailConfig.MailAddress, _emailConfig.MailPassword);
                smtpClient.EnableSsl = true;

                // Send email
                await smtpClient.SendMailAsync(mail);
            }
            catch
            {
                throw new Exception("Email send failed");
            }
        }
        public bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}