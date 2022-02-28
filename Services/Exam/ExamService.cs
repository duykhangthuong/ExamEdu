using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.ExamDTO;
using examedu.Helper.RandomGenerator;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.EntityFrameworkCore;

namespace ExamEdu.Services
{
    public class ExamService : IExamService
    {
        private readonly DataContext _db;
        private readonly int maxMark = 10;
        public ExamService(DataContext dataContext)
        {
            _db = dataContext;
        }
        /// <summary>
        /// Get Exam Schedule of student, which have Exam day > now
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public async Task<Tuple<int, IEnumerable<Exam>>> GetExamByStudentId(int studentId, PaginationParameter paginationParameter)
        {
            var allExamOfStudent = await _db.StudentExamInfos.Where(e => e.StudentId == studentId).Select(e => e.ExamId).ToListAsync();
            if (allExamOfStudent.Count() == 0)
            {
                return null;
            }
            List<Exam> examList = new List<Exam>();
            foreach (var examId in allExamOfStudent)
            {
                var exam = await _db.Exams.FirstOrDefaultAsync(e => e.ExamId == examId && e.ExamDay >= DateTime.Now);
                if (exam is not null)
                {
                    examList.Add(exam);
                }
            }

            return Tuple.Create(examList.Count, examList.GetPage(paginationParameter));
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

        public async Task<Exam> GetExamById(int id)
        {
            return await _db.Exams.Where(e => e.ExamId == id).FirstOrDefaultAsync();
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
            decimal MCQMark = totalMCQMark / totalMCQuestion;


            if (input.IsFinal)
            {
                for (int i = 1; i <= input.VariantNumber; i++)
                {
                    foreach (var level in input.NumberOfMCQuestionByLevel)
                    {
                        List<int> randomList = ChooseRandomFromList.ChooseRandom<int>(
                            await _db.FEQuestions.Where(q => q.LevelId == level.Key && q.QuestionTypeId == 1)
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
                           await _db.FEQuestions.Where(q => q.LevelId == level.Key && q.QuestionTypeId != 1)
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
                            await _db.Questions.Where(q => q.LevelId == level.Key && q.QuestionTypeId == 1)
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
                            await _db.Questions.Where(q => q.LevelId == level.Key && q.QuestionTypeId != 1)
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

    }
}