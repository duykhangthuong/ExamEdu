using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.ExamDTO;
using examedu.DTO.StudentDTO;
using examedu.Helper.RandomGenerator;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ExamDTO;
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

        public async Task<Tuple<int, IEnumerable<Exam>>> GetExamsByClassModuleId(int classModuleId, PaginationParameter paginationParameter)
        {
            // var exams = from cm in _db.ClassModules
            //             join m in _db.Modules on cm.ModuleId equals m.ModuleId
            //             join e in _db.Exams on m.ModuleId equals e.ModuleId
            //             where cm.ClassModuleId == classModuleId
            //             select e;

            // var exams = await _db.ClassModules.Join(_db.Modules, cm => cm.ModuleId, m => m.ModuleId, (cm, m) => new { cm, m })
            //             .Join(_db.Exams, x => x.m.ModuleId, e => e.ModuleId, (x, e) => new { x, e })
            //             .Where(y => y.x.cm.ClassModuleId == classModuleId)
            //             .OrderBy(y => y.e.ExamDay)
            //             .Select(y => y.e).ToListAsync();

            var exams = from cm in _db.ClassModules
                        join cms in _db.Class_Module_Students on cm.ClassModuleId equals cms.ClassModuleId
                        join s in _db.Students on cms.StudentId equals s.StudentId
                        join sei in _db.StudentExamInfos on s.StudentId equals sei.StudentId
                        join e in _db.Exams on sei.ExamId equals e.ExamId
                        where cm.ClassModuleId == classModuleId
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
    }
}