using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using examedu.DTO.ExamDTO;
using examedu.DTO.StudentDTO;
using examedu.Helper.RandomGenerator;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ExamDTO;
using ExamEdu.DTO.MarkDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using ExamEdu.Helper.UploadDownloadFiles;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace ExamEdu.Services
{
    public class ExamService : IExamService
    {
        private readonly DataContext _db;
        private readonly IMegaHelper _megaHelper;
        private readonly int maxMark = 10;
        public ExamService(DataContext dataContext, IMegaHelper megaHelper)
        {
            _db = dataContext;
            _megaHelper = megaHelper;
        }
        /// <summary>
        /// Get Exam Schedule of student, which have Exam day + Duration  > now
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public async Task<Tuple<int, IEnumerable<Exam>>> getExamByStudentId(int studentId, PaginationParameter paginationParameter)
        {

            var examScheduleList = await _db.StudentExamInfos.Join(_db.Exams, sei => sei.ExamId, e => e.ExamId, (sei, e) => new
            {
                sei,
                e
            }).Where(x => x.sei.StudentId == studentId
                        && x.sei.FinishAt == null
                        && x.e.ExamDay.AddMinutes(x.e.DurationInMinute) > DateTime.Now)
                                                                            .Select(x => new Exam
                                                                            {
                                                                                ExamId = x.e.ExamId,
                                                                                ExamName = x.e.ExamName,
                                                                                Description = x.e.Description,
                                                                                Module = new Module
                                                                                {
                                                                                    ModuleCode = x.e.Module.ModuleCode
                                                                                },
                                                                                ExamDay = x.e.ExamDay,
                                                                                Password = x.e.Password,
                                                                                DurationInMinute = x.e.DurationInMinute,
                                                                            }).OrderByDescending(e => e.ExamDay).ToListAsync();
            return Tuple.Create(examScheduleList.Count, examScheduleList.GetPage(paginationParameter));
        }

        /// <summary>
        /// create exampaper random and add to database
        /// </summary>
        /// <param name="input"></param>
        /// <returns>-1 if can not random, 0 if can not add to db, 1 if success</returns>
        public async Task<int> CreateExamPaperByHand(CreateExamByHandInput input)
        {
            decimal totalMCQMark = maxMark;
            foreach (var level in input.MarkByLevel)
            {
                totalMCQMark -= level.Value * input.NumberOfNonMCQuestionByLevel[level.Key];
            }
            int totalMCQuestion = 0;
            foreach (var level in input.NumberOfMCQuestionByLevel)
            {
                totalMCQuestion += level.Value;
            }
            if (totalMCQuestion == 0)
            {
                totalMCQuestion++;
            }
            decimal MCQMark = totalMCQMark / totalMCQuestion;

            if (input.IsFinal)
            {
                for (int i = 1; i <= input.VariantNumber; i++)
                {
                    foreach (var level in input.MCQuestionByLevel)
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(level.Value, input.NumberOfMCQuestionByLevel[level.Key]);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList) //random MCQ
                        {
                            Exam_FEQuestion examQuestion = new Exam_FEQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.FEQuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)MCQMark;
                            await _db.Exam_FEQuestions.AddAsync(examQuestion);
                        }
                    }
                    foreach (var level in input.NonMCQuestionByLevel) // random nonMCQ
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(level.Value, input.NumberOfNonMCQuestionByLevel[level.Key]);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList)
                        {
                            Exam_FEQuestion examQuestion = new Exam_FEQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.FEQuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)input.MarkByLevel[level.Key];
                            await _db.Exam_FEQuestions.AddAsync(examQuestion);
                        }
                    }
                }
            }
            else
            {
                for (int i = 1; i <= input.VariantNumber; i++)
                {
                    foreach (var level in input.MCQuestionByLevel)
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(level.Value, input.NumberOfMCQuestionByLevel[level.Key]);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList) //random MCQ
                        {
                            ExamQuestion examQuestion = new ExamQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.QuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)MCQMark;
                            await _db.ExamQuestions.AddAsync(examQuestion);
                        }
                    }
                    foreach (var level in input.NonMCQuestionByLevel) // random nonMCQ
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(level.Value, input.NumberOfNonMCQuestionByLevel[level.Key]);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList)
                        {
                            ExamQuestion examQuestion = new ExamQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.QuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)input.MarkByLevel[level.Key];
                            await _db.ExamQuestions.AddAsync(examQuestion);
                        }
                    }
                }
            }
            try
            {
                int rowAffted = await _db.SaveChangesAsync();
                if (rowAffted == 0)
                {
                    return -1;
                }
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        public async Task<Exam> getExamById(int id)
        {
            return await _db.Exams.Where(e => e.ExamId == id)
                                .Select(e => new Exam
                                {
                                    ExamId = e.ExamId,
                                    ExamName = e.ExamName,
                                    Description = e.Description,
                                    ExamDay = e.ExamDay,
                                    DurationInMinute = e.DurationInMinute,
                                    CreatedAt = e.CreatedAt,
                                    isFinalExam = e.isFinalExam,
                                    IsCancelled = e.IsCancelled,
                                    ProctorId = e.ProctorId,
                                    SupervisorId = e.SupervisorId,
                                    ModuleId = e.ModuleId,
                                    Room = e.Room,
                                    Module = new Module
                                    {
                                        ModuleCode = e.Module.ModuleCode
                                    }
                                })
                                .FirstOrDefaultAsync();
        }

        public StudentExamInfo GetStudentExamInfo(int studentId, int examId)
        {
            return _db.StudentExamInfos.Where(sei => sei.ExamId == examId && sei.StudentId == studentId).FirstOrDefault();
        }
        public async Task<int> CreateExamPaperAuto(CreateExamAutoInput input)
        {
            decimal totalMCQMark = maxMark;
            foreach (var level in input.MarkByLevel)
            {
                totalMCQMark -= level.Value * input.NumberOfNonMCQuestionByLevel[level.Key];
            }
            int totalMCQuestion = 0;
            foreach (var level in input.NumberOfMCQuestionByLevel)
            {
                totalMCQuestion += level.Value;
            }
            if (totalMCQuestion == 0)
            {
                totalMCQuestion++;
            }
            decimal MCQMark = totalMCQMark / totalMCQuestion;


            if (input.IsFinal)
            {
                for (int i = 1; i <= input.VariantNumber; i++)
                {
                    foreach (var level in input.NumberOfMCQuestionByLevel)
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(
                            await _db.FEQuestions.Where(q => q.LevelId == level.Key && q.QuestionTypeId == 1 && q.isApproved == true)
                            .Select(q => q.FEQuestionId).ToListAsync(), level.Value);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList) //random MCQ
                        {
                            Exam_FEQuestion examQuestion = new Exam_FEQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.FEQuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)MCQMark;
                            await _db.Exam_FEQuestions.AddAsync(examQuestion);
                        }
                    }
                    foreach (var level in input.NumberOfNonMCQuestionByLevel) // random nonMCQ
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(
                           await _db.FEQuestions.Where(q => q.LevelId == level.Key && q.QuestionTypeId != 1 && q.isApproved == true)
                           .Select(q => q.FEQuestionId).ToListAsync(), level.Value);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList)
                        {
                            Exam_FEQuestion examQuestion = new Exam_FEQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.FEQuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)input.MarkByLevel[level.Key];
                            await _db.Exam_FEQuestions.AddAsync(examQuestion);
                        }
                    }
                }
            }
            else
            {
                for (int i = 1; i <= input.VariantNumber; i++)
                {
                    foreach (var level in input.NumberOfMCQuestionByLevel)
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(
                            await _db.Questions.Where(q => q.LevelId == level.Key && q.QuestionTypeId == 1 && q.isApproved == true)
                            .Select(q => q.QuestionId).ToListAsync(), level.Value);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList) //random MCQ
                        {
                            ExamQuestion examQuestion = new ExamQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.QuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)MCQMark;
                            await _db.ExamQuestions.AddAsync(examQuestion);
                        }
                    }
                    foreach (var level in input.NumberOfNonMCQuestionByLevel) // random nonMCQ
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(
                            await _db.Questions.Where(q => q.LevelId == level.Key && q.QuestionTypeId != 1 && q.isApproved == true)
                            .Select(q => q.QuestionId).ToListAsync(), level.Value);
                        if (randomList == null)
                        {
                            return -1;
                        }
                        foreach (var question in randomList)
                        {
                            ExamQuestion examQuestion = new ExamQuestion();
                            examQuestion.ExamId = input.ExamId;
                            examQuestion.QuestionId = question;
                            examQuestion.ExamCode = i;
                            examQuestion.QuestionMark = (float)input.MarkByLevel[level.Key];
                            await _db.ExamQuestions.AddAsync(examQuestion);
                        }
                    }
                }
            }
            try
            {
                int rowAffted = await _db.SaveChangesAsync();
                if (rowAffted == 0)
                {
                    return -1;
                }
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Check if an exam is a final exam
        /// </summary>
        /// <param name="examId"></param>
        /// <returns></returns>
        public bool IsFinalExam(int examId)
        {
            return _db.Exams.Where(e => e.ExamId == examId).Select(e => e.isFinalExam).FirstOrDefault();
        }

        public Tuple<int, IEnumerable<Exam>> GetExamsByClassModuleId(int classModuleId, int moduleId, PaginationParameter paginationParameter)
        {
            var exams = from cm in _db.ClassModules
                        join cms in _db.Class_Module_Students on cm.ClassModuleId equals cms.ClassModuleId
                        join s in _db.Students on cms.StudentId equals s.StudentId
                        join sei in _db.StudentExamInfos on s.StudentId equals sei.StudentId
                        join e in _db.Exams on sei.ExamId equals e.ExamId
                        where cm.ClassModuleId == classModuleId && e.ModuleId == moduleId
                        orderby e.ExamDay descending
                        select e;

            var distinctExams = exams.Distinct();

            int totalRecord = distinctExams.Count();

            return new Tuple<int, IEnumerable<Exam>>(totalRecord, distinctExams.OrderByDescending(e => e.ExamDay).GetPage(paginationParameter));
        }


        /// <summary>
        /// Insert information about an exam into the database
        /// </summary>
        /// <param name="exam">Information of the exam</param>
        /// <returns>
        /// A tuple of two values:
        ///    - The first value is the number of rows affected
        ///   - The second value is the examId of the exam inserted
        /// </returns>
        public async Task<Tuple<int, int>> CreateExamInfo(Exam exam)
        {
            //verify exam name is unique
            if(_db.Exams.Any(e => e.ExamName.Equals(exam.ExamName)))
            {
                throw new Exception("Exam name already exist");
            }

            //Insert exam to database
            await _db.Exams.AddAsync(exam);
            int result = await _db.SaveChangesAsync();
            return new Tuple<int, int>(result, exam.ExamId);

        }
        public async Task<Tuple<int, IEnumerable<StudentMarkResponse>>> GetResultExamByExamId(int examId, PaginationParameter paginationParameter)
        {
            var studentExamInfor = await _db.StudentExamInfos.Join(_db.Exams, sei => sei.ExamId, e => e.ExamId, (sei, e) => new { sei, e })
                                                            .Join(_db.Students, x => x.sei.StudentId, s => s.StudentId, (x, s) => new { x, s })
                                                            .Where(y => y.x.sei.ExamId == examId && y.x.sei.FinishAt != null)
                                                            .Select(y => new StudentMarkResponse
                                                            {
                                                                ExamName = y.x.e.ExamName,
                                                                ExamDay = y.x.e.ExamDay,
                                                                StudentId = y.s.StudentId,
                                                                StudentName = y.s.Fullname,
                                                                StudentEmail = y.s.Email,
                                                                FinishedAt = y.x.sei.FinishAt,
                                                                Mark = y.x.sei.Mark,
                                                                NeedToGradeTextQuestion = y.x.sei.NeedToGradeTextQuestion,
                                                            }).ToListAsync();
            int totalRecord = studentExamInfor.Count;

            return new Tuple<int, IEnumerable<StudentMarkResponse>>(totalRecord, studentExamInfor.GetPage(paginationParameter));
        }

        public async Task<IEnumerable<StudentMarkResponse>> GetResultExamListByExamId(int examId)
        {
            var studentExamInfor = await _db.StudentExamInfos.Join(_db.Exams, sei => sei.ExamId, e => e.ExamId, (sei, e) => new { sei, e })
                                                            .Join(_db.Students, x => x.sei.StudentId, s => s.StudentId, (x, s) => new { x, s })
                                                            .Where(y => y.x.sei.ExamId == examId && y.x.sei.FinishAt != null)
                                                            .Select(y => new StudentMarkResponse
                                                            {
                                                                ExamName = y.x.e.ExamName,
                                                                ExamDay = y.x.e.ExamDay,
                                                                StudentId = y.s.StudentId,
                                                                StudentName = y.s.Fullname,
                                                                StudentEmail = y.s.Email,
                                                                FinishedAt = y.x.sei.FinishAt,
                                                                Mark = y.x.sei.Mark,
                                                                NeedToGradeTextQuestion = y.x.sei.NeedToGradeTextQuestion,
                                                            }).ToListAsync();

            return studentExamInfor;
        }
        public async Task<ClassModule> GetClassModuleInfo(int classModuleId)
        {
            //Get class module info from classModuleId
            var queryResult = from cm in _db.ClassModules
                              join c in _db.Classes on cm.ClassId equals c.ClassId
                              join m in _db.Modules on cm.ModuleId equals m.ModuleId
                              where cm.ClassModuleId == classModuleId
                              select new ClassModule
                              {
                                  ClassModuleId = cm.ClassModuleId,
                                  Class = new Class
                                  {
                                      ClassId = c.ClassId,
                                      ClassName = c.ClassName
                                  },
                                  Module = new Module
                                  {
                                      ModuleId = m.ModuleId,
                                      ModuleCode = m.ModuleCode,
                                      ModuleName = m.ModuleName
                                  }
                              };
            var classModule = await queryResult.FirstOrDefaultAsync();

            return classModule;
        }
        public async Task<byte[]> GenerateExamMarkReport(int examId, int classModuleId)
        {
            var studentMarkResult = await this.GetResultExamListByExamId(examId);
            var classModules = await this.GetClassModuleInfo(classModuleId);

            // using var stream = File.OpenRead(pathToTest);
            using var stream = await _megaHelper.Download(new Uri("https://mega.nz/file/zsNCVbYQ#zfVii8j29x6UUTm-cxTYiZHA6C3l08MFQPuCLST97qI"));
            using (var package = new ExcelPackage())
            {
                await package.LoadAsync(stream);
                var examMarkReport = package.Workbook.Worksheets[0];

                examMarkReport.Cells["A2"].Value = classModules.Class.ClassName;
                examMarkReport.Cells["A3"].Value = $"{classModules.Module.ModuleCode} - {classModules.Module.ModuleName}";
                examMarkReport.Cells["C2"].Value = studentMarkResult.First().ExamName;
                examMarkReport.Cells["C3"].Value = studentMarkResult.First().ExamDay;

                var studentMarkResultFill = examMarkReport.Cells[$"A6:E{studentMarkResult.Count() + 6}"];
                studentMarkResultFill.FillDataToCells(studentMarkResult, (markInfo, cells) =>
                {
                    cells.ToList().ForEach(c => c.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin));
                    cells[0].Value = markInfo.StudentId;
                    cells[1].Value = markInfo.StudentName;
                    cells[2].Value = markInfo.FinishedAt - markInfo.ExamDay;
                    cells[3].Value = markInfo.FinishedAt;
                    cells[4].Value = markInfo.Mark;
                });

                return await package.GetAsByteArrayAsync();
            }
        }

        /// <summary>
        /// Update the information of the exam
        /// </summary>
        /// <param name="exam"></param>
        /// <returns></returns>
        public async Task<int> UpdateExam(Exam exam)
        {
            //Checking exam name is unique
            if(_db.Exams.Any(e => e.ExamName.Equals(exam.ExamName)))
            {
                throw new Exception("Exam name already exist");
            }

            _db.Exams.Attach(exam);
            var entry = _db.Entry(exam);

            entry.Property(e => e.ExamName).IsModified = true;
            entry.Property(e => e.ExamDay).IsModified = true;
            entry.Property(e => e.DurationInMinute).IsModified = true;
            entry.Property(e => e.Description).IsModified = true;
            entry.Property(e => e.Password).IsModified = true;
            entry.Property(e => e.Room).IsModified = true;
            entry.Property(e => e.ModuleId).IsModified = true;
            entry.Property(e => e.ProctorId).IsModified = true;
            entry.Property(e => e.SupervisorId).IsModified = true;
            entry.Property(e => e.isFinalExam).IsModified = true;

            int result = await _db.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<int> UpdateExamRoom(int examId, string roomId)
        {
            var exam = await _db.Exams.Where(c => c.ExamId == examId).FirstOrDefaultAsync();
            if (exam == null)
            {
                return 0;
            }
            exam.Room = roomId;
            int result = await _db.SaveChangesAsync();
            return result;
        }

        public async Task<Exam> GetUpdateExam(int examId)
        {
            Exam exam = await _db.Exams.Where(c => c.ExamId == examId).FirstOrDefaultAsync();

            return exam;
        }

        public async Task<Tuple<int, IEnumerable<Exam>>> GetAllExam(PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");
            searchName = ConvertToUnsign(searchName);
            var queryResult = from e in _db.Exams
                              join m in _db.Modules on e.ModuleId equals m.ModuleId
                              where e.ExamName.ToLower().Contains(searchName.ToLower()) || m.ModuleCode.ToLower().Contains(searchName.ToLower())
                              orderby e.ExamDay descending
                              select new Exam
                              {
                                  ExamId = e.ExamId,
                                  ExamName = e.ExamName,
                                  ExamDay = e.ExamDay,
                                  Module = new Module
                                  {
                                      ModuleId = e.ModuleId,
                                      ModuleCode = m.ModuleCode,
                                      ModuleName = m.ModuleName,
                                  },
                              };

            var exams = await queryResult.ToListAsync();

            var totalCount = exams.Count;

            return new Tuple<int, IEnumerable<Exam>>(totalCount, exams.GetPage(paginationParameter));
        }

        private string ConvertToUnsign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty)
                        .Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public bool IsCancelled(int examId)
        {
            return _db.Exams.Where(e => e.ExamId == examId && e.IsCancelled == true).Any();
        }

        public bool IsExist(int examId)
        {
            return _db.Exams.Where(s => s.ExamId == examId && s.IsCancelled == false).Any();
        }

        public async Task<int> CancelExam(int examId)
        {
            var examFound = await _db.Exams.Where(e => e.ExamId == examId).FirstOrDefaultAsync();

            if (examFound == null)
            {
                return 0;
            }
            //check if the exam is over or in the future
            if (examFound.ExamDay.CompareTo(DateTime.Now) < 0)
            {
                return -1;
            }
            examFound.IsCancelled = true;
            int result = await _db.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<Student>> GetStudents(int classModuleId)
        {
            var queryResult = from student in _db.Students
                              join cms in _db.Class_Module_Students on student.StudentId equals cms.StudentId
                              where cms.ClassModuleId == classModuleId && student.DeactivatedAt == null
                              orderby student.StudentId ascending
                              select new Student
                              {
                                  StudentId = student.StudentId,
                                  Fullname = student.Fullname
                              };
            var students = await queryResult.ToListAsync();

            return students; ;
        }

        // Get all exam by classModuleId and moduleId include exam result
        public async Task<IEnumerable<ProgressExamReport>> GetAllExamResultByClassModuleId(int classModuleId, int moduleId)
        {
            var examsGet = from cm in _db.ClassModules
                           join cms in _db.Class_Module_Students on cm.ClassModuleId equals cms.ClassModuleId
                           join s in _db.Students on cms.StudentId equals s.StudentId
                           join sei in _db.StudentExamInfos on s.StudentId equals sei.StudentId
                           join e in _db.Exams on sei.ExamId equals e.ExamId
                           where cm.ClassModuleId == classModuleId && e.ModuleId == moduleId && e.isFinalExam == false
                           orderby e.ExamId descending
                           select new ProgressExamReport
                           {
                               ExamId = e.ExamId,
                               ExamName = e.ExamName,
                               StudentMark = new List<StudentProgressResult>()
                           };
            var exams = examsGet.Distinct().ToList();
            var students = await this.GetStudents(classModuleId);
            foreach (var e in exams)
            {
                foreach (var st in students)
                {
                    var examResult = await _db.StudentExamInfos.Join(_db.Students,
                                                                     sei => sei.StudentId,
                                                                     s => s.StudentId,
                                                                     (sei, s) => new { sei, s })
                                                                     .Where(x => x.sei.StudentId == st.StudentId
                                                                             && x.sei.ExamId == e.ExamId)
                                                                     .Select(x => new StudentProgressResult
                                                                     {
                                                                         StudentId = x.sei.StudentId,
                                                                         StudentName = x.s.Fullname,
                                                                         Mark = x.sei.Mark
                                                                     })
                                                                    .FirstOrDefaultAsync();
                    if (examResult == null)
                    {
                        e.StudentMark.Add(new StudentProgressResult
                        {
                            StudentId = st.StudentId,
                            StudentName = st.Fullname,
                            Mark = null
                        });
                    }
                    else
                    {
                        e.StudentMark.Add(examResult);
                    }
                }
            }
            return exams;
        }
        public async Task<byte[]> GenerateModuleProgressExamReport(int classModuleId, int moduleId)
        {
            var classModules = await this.GetClassModuleInfo(classModuleId);
            var students = await this.GetStudents(classModuleId);
            var examsWithResult = await this.GetAllExamResultByClassModuleId(classModuleId, moduleId);
            // using var stream = File.OpenRead(pathToTest);
            using var stream = await _megaHelper.Download(new Uri("https://mega.nz/file/Lh0HGaYL#eHVmZPVAUNN6eD3WfquteM0CmK4DELbhR5QthJTbTvU"));
            using (var package = new ExcelPackage())
            {
                await package.LoadAsync(stream);
                var moduleProgressExam = package.Workbook.Worksheets[0];

                moduleProgressExam.Cells["A2"].Value = classModules.Class.ClassName;
                moduleProgressExam.Cells["A3"].Value = $"{classModules.Module.ModuleCode} - {classModules.Module.ModuleName}";

                var studentsFill = moduleProgressExam.Cells[$"A6:B{students.Count() + 6}"];
                studentsFill.FillDataToCells(students, (markInfo, cells) =>
                {
                    cells[0].Value = markInfo.StudentId;
                    cells[1].Value = markInfo.Fullname;
                });

                // Get ra từng column để điền exam vào, sau đó moveRight để điền tiếp exam tiếp theo
                var examResultFill = moduleProgressExam.Cells[$"C5:C{students.Count() + 5}"];
                // loop qua list exam để fill data
                foreach (var exam in examsWithResult)
                {
                    //Set exam name in the first row (header exam)
                    examResultFill.SetCellValue(0, 0, exam.ExamName);
                    // với mỗi exam, loop qua danh sách student để fill data
                    foreach (var (item, index) in exam.StudentMark.Select((value, i) => (value, i)))
                    {
                        // fill data của từng student mark
                        // index +1 vì đã điền exam name ở ô đầu tiên
                        examResultFill.SetCellValue(index + 1, 0, item.Mark);
                    }
                    examResultFill = examResultFill.MoveRight(1);
                }
                return await package.GetAsByteArrayAsync();
            }

        }
        public async Task<Exam> GetExamDetailByExamId(int examId)
        {
            return await _db.Exams.Where(e => e.ExamId == examId)
                            .Select(e => new Exam
                            {
                                ExamName = e.ExamName,
                                Module = e.Module,
                                Description = e.Description,
                                Password = e.Password,
                                ExamDay = e.ExamDay,
                                CreatedAt = e.CreatedAt,
                                DurationInMinute = e.DurationInMinute,
                                Room = e.Room,
                                isFinalExam = e.isFinalExam,
                                IsCancelled = e.IsCancelled,
                                Proctor = e.Proctor,
                                Supervisor = e.Supervisor,
                            }).FirstOrDefaultAsync();
        }

        public async Task<Tuple<int, IEnumerable<Exam>>> GetExamByProctorId(int proctorId, PaginationParameter paginationParameter)
        {
            var examList = await _db.Exams
                                .Where(e => e.ProctorId == proctorId && 
                                        (e.ExamName.ToUpper().Contains(paginationParameter.SearchName.ToUpper()) 
                                        || e.Module.ModuleCode.ToUpper().Contains(paginationParameter.SearchName.ToUpper())))
                                .Select(e => new Exam
                                {
                                    ExamId = e.ExamId,
                                    ExamName = e.ExamName,
                                    Module = e.Module,
                                    Description = e.Description,
                                    Password = e.Password,
                                    ExamDay = e.ExamDay,
                                    DurationInMinute = e.DurationInMinute,
                                    Room = e.Room,
                                    isFinalExam = e.isFinalExam,
                                    IsCancelled = e.IsCancelled,
                                    Supervisor = e.Supervisor,
                                })
                                .OrderByDescending(e => e.ExamDay)
                                .ToListAsync();
            return new Tuple<int, IEnumerable<Exam>>(examList.Count(), examList.GetPage(paginationParameter));
        }
    }
}