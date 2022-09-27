using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.AccountDTO;
using BackEnd.DTO.Email;
using BackEnd.Helper.Email;
using examedu.DTO.AccountDTO;
using examedu.DTO.ExcelDTO;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace examedu.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly IEmailHelper _emailHelper;

        public AccountService(DataContext dataContext, IMapper mapper, IEmailHelper emailHelper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _emailHelper = emailHelper;
        }

        private string ConvertToUnsign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty)
                        .Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        string getRoleName(int id)
        {
            Role role = _dataContext.Roles.Find(id);
            return role.RoleName;
        }

        private IEnumerable<AccountResponse> addAccountToTotalList(
        IEnumerable<Student> students, IEnumerable<AcademicDepartment> AcademicDepartments, IEnumerable<Teacher> teachers)
        {
            List<AccountResponse> totalAccount = new List<AccountResponse>();

            foreach (var item in students)
            {
                var itemToResponse = _mapper.Map<AccountResponse>(item);
                itemToResponse.RoleName = getRoleName(item.RoleId);
                totalAccount.Add(itemToResponse);
            }

            foreach (var item in AcademicDepartments)
            {
                var itemToResponse = _mapper.Map<AccountResponse>(item);
                itemToResponse.RoleName = getRoleName(item.RoleId);
                totalAccount.Add(itemToResponse);
            }

            foreach (var item in teachers)
            {
                var itemToResponse = _mapper.Map<AccountResponse>(item);
                itemToResponse.RoleName = getRoleName(item.RoleId);
                totalAccount.Add(itemToResponse);
            }

            totalAccount.Sort((x, y) => y.CreatedAt.CompareTo(x.CreatedAt));
            return totalAccount;
        }

        /// <summary>
        /// get all account in all role exepct admin
        /// </summary>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public Tuple<int, IEnumerable<AccountResponse>> GetAccountList(PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");
            searchName = ConvertToUnsign(searchName);

            IEnumerable<Student> studentList = _dataContext.Students.ToList().Where(t
                 => t.DeactivatedAt == null && (t.Email.ToUpper().Contains(searchName.ToUpper())
                || t.Fullname.ToUpper().Contains(searchName.ToUpper())
                || ConvertToUnsign(t.Fullname).ToUpper().Contains(searchName.ToUpper())));
            IEnumerable<AcademicDepartment> AcademicDepartmentList = _dataContext.AcademicDepartments.ToList().Where(t
                 => t.DeactivatedAt == null && (t.Email.ToUpper().Contains(searchName.ToUpper())));
            IEnumerable<Teacher> teacherList = _dataContext.Teachers.ToList().Where(t
                 => t.DeactivatedAt == null && (t.Email.ToUpper().Contains(searchName.ToUpper())
                || t.Fullname.ToUpper().Contains(searchName.ToUpper())
                || ConvertToUnsign(t.Fullname).ToUpper().Contains(searchName.ToUpper())));

            IEnumerable<AccountResponse> totalAccount = addAccountToTotalList(studentList, AcademicDepartmentList, teacherList);

            return Tuple.Create(totalAccount.Count(), PaginationHelper.GetPage(totalAccount,
                paginationParameter.PageSize, paginationParameter.PageNumber));
        }

        /// <summary>
        /// get all DeactivatedAccount in all role except admin
        /// </summary>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public Tuple<int, IEnumerable<AccountResponse>> GetDeactivatedAccountList(PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");
            searchName = ConvertToUnsign(searchName);

            IEnumerable<Student> studentList = _dataContext.Students.ToList().Where(t
                 => t.DeactivatedAt != null && (t.Email.ToUpper().Contains(searchName.ToUpper())
                || t.Fullname.ToUpper().Contains(searchName.ToUpper())
                || ConvertToUnsign(t.Fullname).ToUpper().Contains(searchName.ToUpper())));
            IEnumerable<AcademicDepartment> AcademicDepartmentList = _dataContext.AcademicDepartments.ToList().Where(t
                 => t.DeactivatedAt != null && (t.Email.ToUpper().Contains(searchName.ToUpper())));
            IEnumerable<Teacher> teacherList = _dataContext.Teachers.ToList().Where(t
                 => t.DeactivatedAt != null && (t.Email.ToUpper().Contains(searchName.ToUpper())
                || t.Fullname.ToUpper().Contains(searchName.ToUpper())
                || ConvertToUnsign(t.Fullname).ToUpper().Contains(searchName.ToUpper())));

            IEnumerable<AccountResponse> totalAccount = addAccountToTotalList(studentList, AcademicDepartmentList, teacherList);

            return Tuple.Create(totalAccount.Count(), PaginationHelper.GetPage(totalAccount,
                paginationParameter.PageSize, paginationParameter.PageNumber));
        }

        private void sendEmail(string email, string password)
        {
            EmailContent emailContent = new EmailContent();
            emailContent.IsBodyHtml = true;
            emailContent.ToEmail = email;
            emailContent.Subject = "[ExamEdu] Your Password";
            emailContent.Body = password;
            _emailHelper.SendEmailAsync(emailContent);
        }

        private string processPasswordAndSendEmail(string email)
        {
            string password = AutoGeneratorPassword.passwordGenerator(15, 5, 5, 5);

            sendEmail(email, password);
            password = BCrypt.Net.BCrypt.HashPassword(password);

            return password;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountInput"></param>
        /// <returns>1 if sucess / 0 if fail(duplicate email) / -1 all other fail (invalid role...)</returns>
        public async Task<int> InsertNewAccount(AccountInput accountInput)
        {
            PaginationParameter paginationParameter = new PaginationParameter { PageNumber = 1, PageSize = 1, SearchName = accountInput.Email };
            if (GetAccountList(paginationParameter).Item1 >= 1 || GetDeactivatedAccountList(paginationParameter).Item1 >= 1)
            {
                return 0;
            }

            switch (accountInput.RoleID)
            {
                //CASE NAY CHI DUNG KHI MUON TAO TAI KHOAN ADMIN KO BO CMT CASE NAY
                // case 0:
                //     var adminToAdd = _mapper.Map<Administrator>(accountInput);
                //     adminToAdd.RoleId = accountInput.RoleID;
                //     adminToAdd.Password = processPasswordAndSendEmail(adminToAdd.Email);
                //     _dataContext.Administrators.Add(adminToAdd);
                //     if(await _dataContext.SaveChangesAsync() !=1)
                //     {
                //         return -1;
                //     }
                //     else
                //     {
                //         return 1;
                //     }
                case 1:
                    var studentToAdd = _mapper.Map<Student>(accountInput);
                    studentToAdd.RoleId = accountInput.RoleID;
                    studentToAdd.Password = processPasswordAndSendEmail(studentToAdd.Email);
                    _dataContext.Students.Add(studentToAdd);
                    if (await _dataContext.SaveChangesAsync() != 1)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                case 2:
                    var teacherToAdd = _mapper.Map<Teacher>(accountInput);
                    teacherToAdd.RoleId = accountInput.RoleID;
                    teacherToAdd.Password = processPasswordAndSendEmail(teacherToAdd.Email);
                    _dataContext.Teachers.Add(teacherToAdd);
                    if (await _dataContext.SaveChangesAsync() != 1)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                case 3:
                    var academicDepartToAdd = _mapper.Map<AcademicDepartment>(accountInput);
                    academicDepartToAdd.RoleId = accountInput.RoleID;
                    academicDepartToAdd.Password = processPasswordAndSendEmail(academicDepartToAdd.Email);
                    _dataContext.AcademicDepartments.Add(academicDepartToAdd);
                    if (await _dataContext.SaveChangesAsync() != 1)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                default:
                    return -1;
            }
        }

        public int InsertNewAccountNoSaveChange(AccountInput accountInput)
        {
            PaginationParameter paginationParameter = new PaginationParameter { PageNumber = 1, PageSize = 1, SearchName = accountInput.Email };
            if (GetAccountList(paginationParameter).Item1 >= 1 || GetDeactivatedAccountList(paginationParameter).Item1 >= 1)
            {
                return 0;
            }

            switch (accountInput.RoleID)
            {
                //CASE NAY CHI DUNG KHI MUON TAO TAI KHOAN ADMIN KO BO CMT CASE NAY
                // case 0:
                //     var adminToAdd = _mapper.Map<Administrator>(accountInput);
                //     adminToAdd.RoleId = accountInput.RoleID;
                //     adminToAdd.Password = processPasswordAndSendEmail(adminToAdd.Email);
                //     _dataContext.Administrators.Add(adminToAdd);
                //     if(await _dataContext.SaveChangesAsync() !=1)
                //     {
                //         return -1;
                //     }
                //     else
                //     {
                //         return 1;
                //     }
                case 1:
                    var studentToAdd = _mapper.Map<Student>(accountInput);
                    studentToAdd.RoleId = accountInput.RoleID;
                    studentToAdd.Password = processPasswordAndSendEmail(studentToAdd.Email);
                    _dataContext.Students.Add(studentToAdd);
                    return 1;
                case 2:
                    var teacherToAdd = _mapper.Map<Teacher>(accountInput);
                    teacherToAdd.RoleId = accountInput.RoleID;
                    teacherToAdd.Password = processPasswordAndSendEmail(teacherToAdd.Email);
                    _dataContext.Teachers.Add(teacherToAdd);
                    return 1;
                case 3:
                    var academicDepartToAdd = _mapper.Map<AcademicDepartment>(accountInput);
                    academicDepartToAdd.RoleId = accountInput.RoleID;
                    academicDepartToAdd.Password = processPasswordAndSendEmail(academicDepartToAdd.Email);
                    return 1;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Deactivate base on role
        /// </summary>
        /// <param name="id">id of user</param>
        /// <param name="role">role of user</param>
        /// <returns>-1:Already Deactivated / 0:fail / 1:success</returns>
        public async Task<int> DeactivateAccount(int id, int roleID)
        {
            switch (roleID)
            {
                case 1:
                    var studentToDeActivate = await _dataContext.Students.Where(s =>
                         s.DeactivatedAt == null && s.StudentId == id).FirstOrDefaultAsync();
                    if (studentToDeActivate == null)
                    {
                        return -1;
                    }
                    studentToDeActivate.DeactivatedAt = DateTime.Now;
                    return await _dataContext.SaveChangesAsync();
                case 2:
                    var teacherToDeActivate = await _dataContext.Teachers.Where(s =>
                         s.DeactivatedAt == null && s.TeacherId == id).FirstOrDefaultAsync();
                    if (teacherToDeActivate == null)
                    {
                        return -1;
                    }
                    teacherToDeActivate.DeactivatedAt = DateTime.Now;
                    return await _dataContext.SaveChangesAsync();
                case 3:
                    var AcademicDepartToDeActivate = await _dataContext.AcademicDepartments.Where(s =>
                         s.DeactivatedAt == null && s.AcademicDepartmentId == id).FirstOrDefaultAsync();
                    if (AcademicDepartToDeActivate == null)
                    {
                        return -1;
                    }
                    AcademicDepartToDeActivate.DeactivatedAt = DateTime.Now;
                    return await _dataContext.SaveChangesAsync();
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Get account and password in 4 roles by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Tuple<AccountResponse, string>> GetAccountByEmail(string email)
        {
            string password = "";
            AccountResponse accountToReponse;

            // async Task<string> getRoleName(int id)
            // {
            //     Role role = await _dataContext.Roles.FindAsync(id);
            //     return role.RoleName;
            // }

            Administrator administrator = await _dataContext.Administrators.Where(s => s.Email.ToLower().Equals(email.ToLower())).FirstOrDefaultAsync(); ;
            if (administrator != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(administrator);
                // accountToReponse.Role = await getRoleName(administrator.RoleId);
                // accountToReponse.ID = administrator.AdministratorId;
                password = administrator.Password;
                return Tuple.Create(accountToReponse, password);
            }

            Student student = await _dataContext.Students.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (student != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(student);
                // accountToReponse.Role = await getRoleName(student.RoleId);
                // accountToReponse.ID = student.StudentId;
                password = student.Password;
                return Tuple.Create(accountToReponse, password);
            }

            Teacher teacher = await _dataContext.Teachers.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (teacher != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(teacher);
                // accountToReponse.Role = await getRoleName(teacher.RoleId);
                // accountToReponse.ID = teacher.TeacherId;
                password = teacher.Password;
                return Tuple.Create(accountToReponse, password);
            }

            AcademicDepartment academicDepartment = await _dataContext.AcademicDepartments.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (academicDepartment != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(academicDepartment);
                // accountToReponse.Role = await getRoleName(academicDepartment.RoleId);
                // accountToReponse.ID = academicDepartment.AcademicDepartmentId;
                password = academicDepartment.Password;
                return Tuple.Create(accountToReponse, password);
            }

            return null;
        }

        public async Task<string> GetRoleName(int id)
        {
            Role role = await _dataContext.Roles.FindAsync(id);
            return role.RoleName;
        }

        public async Task<AccountResponse> GetAccountInforByEmail(string email)
        {
            AccountResponse accountToReponse;

            Administrator administrator = await _dataContext.Administrators.Where(s => s.Email.ToLower().Equals(email.ToLower())).FirstOrDefaultAsync(); ;
            if (administrator != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(administrator);
                accountToReponse.RoleName = await GetRoleName(administrator.RoleId);
                return accountToReponse;
            }

            Student student = await _dataContext.Students.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (student != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(student);
                accountToReponse.RoleName = await GetRoleName(student.RoleId);
                return accountToReponse;
            }

            Teacher teacher = await _dataContext.Teachers.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (teacher != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(teacher);
                accountToReponse.RoleName = await GetRoleName(teacher.RoleId);
                return accountToReponse;
            }

            AcademicDepartment academicDepartment = await _dataContext.AcademicDepartments.Where(s => s.Email.ToLower().Equals(email.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
            if (academicDepartment != null)
            {
                accountToReponse = _mapper.Map<AccountResponse>(academicDepartment);
                accountToReponse.RoleName = await GetRoleName(academicDepartment.RoleId);
                return accountToReponse;
            }

            return null;
        }

        public async Task<int> UpdateAccount(UpdateAccountInput accountInput, int roleId, string currEmail)
        {
            int rowUpdated = 0;
            switch (roleId)
            {
                case 1:
                    Student student = await _dataContext.Students.Where(s => s.Email.ToLower().Equals(currEmail.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
                    if (student == null)
                    {
                        return -1;
                    }
                    student.Email = accountInput.Email;
                    student.Fullname = accountInput.Fullname;

                    rowUpdated = await _dataContext.SaveChangesAsync();
                    return rowUpdated;
                case 2:
                    Teacher teacher = await _dataContext.Teachers.Where(s => s.Email.ToLower().Equals(currEmail.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
                    if (teacher == null)
                    {
                        return -1;
                    }
                    teacher.Email = accountInput.Email;
                    teacher.Fullname = accountInput.Fullname;
                    rowUpdated = await _dataContext.SaveChangesAsync();
                    return rowUpdated;
                case 3:
                    AcademicDepartment academicDepartment = await _dataContext.AcademicDepartments.Where(s => s.Email.ToLower().Equals(currEmail.ToLower()) && s.DeactivatedAt == null).FirstOrDefaultAsync();
                    if (academicDepartment == null)
                    {
                        return -1;
                    }
                    academicDepartment.Email = accountInput.Email;
                    rowUpdated = await _dataContext.SaveChangesAsync();
                    return rowUpdated;
                case 4:
                    Administrator administrator = await _dataContext.Administrators.Where(s => s.Email.ToLower().Equals(currEmail.ToLower())).FirstOrDefaultAsync();
                    if (administrator == null)
                    {
                        return -1;
                    }
                    administrator.Email = accountInput.Email;
                    administrator.Fullname = accountInput.Fullname;
                    rowUpdated = await _dataContext.SaveChangesAsync();
                    return rowUpdated;
                default:
                    return rowUpdated;
            }
        }

        public async Task<Tuple<List<CellErrorInfor>, List<AccountInput>>> convertExcelToAccountInputList(IFormFile excelFile)
        {
            List<AccountInput> listAccountReturn = new List<AccountInput>();
            List<CellErrorInfor> cellErrorInfors = new List<CellErrorInfor>();
            using (MemoryStream ms = new MemoryStream())
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                await excelFile.CopyToAsync(ms);

                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                    var totalRows = workSheet.Dimension.Rows;
                    var totalColumn = workSheet.Dimension.Columns;

                    for (int i = 2; i <= totalRows; i++)
                    {
                        AccountInput tempAccount = new AccountInput();

                        try
                        {
                            tempAccount.Email = workSheet.Cells[i, 1].Value.ToString();
                        }
                        catch (System.Exception)
                        {
                            cellErrorInfors.Add(new CellErrorInfor
                            {
                                RowIndex = i,
                                ColumnIndex = 1,
                                ErrorDetail = "The cell does not have value"
                            });
                        }

                        try
                        {
                            tempAccount.Fullname = workSheet.Cells[i, 2].Value.ToString();
                        }
                        catch (System.Exception)
                        {
                            cellErrorInfors.Add(new CellErrorInfor
                            {
                                RowIndex = i,
                                ColumnIndex = 2,
                                ErrorDetail = "The cell does not have value"
                            });
                        }
                        if (tempAccount.Email != null && !_emailHelper.IsValidEmail(tempAccount.Email))
                        {
                            cellErrorInfors.Add(new CellErrorInfor
                            {
                                RowIndex = i,
                                ColumnIndex = 1,
                                ErrorDetail = "The email is not in valid format"
                            });
                        }
                        listAccountReturn.Add(tempAccount);
                    }
                }
            }
            var duplicateEmail = listAccountReturn
              .Select((t, i) => new { Index = i, Text = t.Email })
              .GroupBy(g => g.Text)
              .Where(g => g.Count() > 1);

            foreach (var item in duplicateEmail)
            {
                foreach (var item2 in item)
                {
                    cellErrorInfors.Add(new CellErrorInfor
                    {
                        RowIndex = item2.Index+2,
                        ColumnIndex = 1,
                        ErrorDetail = "The email is duplicate"
                    });
                }
            }
            return Tuple.Create(cellErrorInfors, listAccountReturn);
        }

        public async Task<Tuple<int, List<CellErrorInfor>>> InsertListAccount(List<AccountInput> listAccount)
        {
            List<CellErrorInfor> cellErrorInfors = new List<CellErrorInfor>();
            for (int i = 1; i <= listAccount.Count; i++)
            {
                if (InsertNewAccountNoSaveChange(listAccount[i - 1]) == 0)
                {
                    cellErrorInfors.Add(new CellErrorInfor
                    {
                        RowIndex = i + 1,
                        ColumnIndex = 1,
                        ErrorDetail = "The email is existed"
                    });
                }
            }
            int numOfRowInserted = 0;
            if (cellErrorInfors.Count == 0)
            {
                numOfRowInserted = await _dataContext.SaveChangesAsync();
            }
            return Tuple.Create(numOfRowInserted, cellErrorInfors);
        }
    }
}