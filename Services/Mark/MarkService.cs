using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services
{
    public class MarkService : IMarkService
    {
        private readonly DataContext _db;
        public MarkService(DataContext db)
        {
            _db = db;
        }

        private class StudentAnswerDetail
        {
            public int StudentAnswerContent { get; set; }
            public decimal QuestionMark { get; set; }
        }
        /// <summary>
        /// get student mark of an given exam (only MCQ)
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="studentId"></param>
        /// <returns>(Status code: 0 if faile, 1 if success)(mark)</returns>
        public async Task<Tuple<int, decimal>> getMCQMarkNonFinal(int examId, int studentId)
        {
            var studentAnswersList = await _db.StudentAnswers.Join(_db.ExamQuestions,
                                                e => e.ExamQuestionId,
                                                s => s.ExamQuestionId,
                                                (s, e) => new
                                                {
                                                    StudentAnswer = s.StudentAnswerContent,
                                                    StudentID = s.StudentId,
                                                    ExamQuestionID = s.ExamQuestionId,
                                                    ExamID = e.ExamId,
                                                    QuestionID = e.QuestionId,
                                                    QuestionMark = e.QuestionMark
                                                }).Where(s => s.StudentID == studentId && s.ExamID == examId)
                                                .Join(_db.Questions,
                                                sa => sa.QuestionID,
                                                q => q.QuestionId,
                                                (sa, q) => new
                                                {
                                                    StudentAnswer = sa.StudentAnswer,
                                                    studentId = sa.StudentID,
                                                    ExamQuestionID = sa.ExamQuestionID,
                                                    QuestionTypeID = q.QuestionTypeId,
                                                    QuestionID = sa.QuestionID,
                                                    QuestionMark = sa.QuestionMark
                                                }).Where(s => s.QuestionTypeID == 1)
                                                .Select(sa => new StudentAnswerDetail
                                                {
                                                    StudentAnswerContent = Int32.Parse(sa.StudentAnswer),
                                                    QuestionMark = (decimal)sa.QuestionMark
                                                }).ToListAsync();

            if (studentAnswersList.Count() == 0 || studentAnswersList == null)
            {
                return Tuple.Create(0, (decimal)0);
            }

            decimal totalMark = 0;

            foreach (var studentAs in studentAnswersList)
            {
                var answer = await _db.Answers.Where(a => a.AnswerId == studentAs.StudentAnswerContent).FirstOrDefaultAsync();
                if (answer != null && answer.isCorrect == true)
                {
                    totalMark += studentAs.QuestionMark;
                }
            }
            totalMark = Math.Round(totalMark, 2);
            return Tuple.Create(1, totalMark);
        }

        /// <summary>
        /// get student mark of an given exam (only MCQ)
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="studentId"></param>
        /// <returns>(Status code: 0 if faile, 1 if success)(mark)</returns>
        public async Task<Tuple<int, decimal>> getMCQMarkFinal(int examId, int studentId)
        {
            var studentAnswersList = await _db.StudentFEAnswers.Join(_db.Exam_FEQuestions,
                                                e => e.ExamFEQuestionId,
                                                s => s.ExamFEQuestionId,
                                                (s, e) => new
                                                {
                                                    StudentAnswer = s.StudentAnswerContent,
                                                    StudentID = s.StudentId,
                                                    ExamQuestionID = s.ExamFEQuestionId,
                                                    ExamID = e.ExamId,
                                                    QuestionID = e.FEQuestionId,
                                                    QuestionMark = e.QuestionMark
                                                }).Where(s => s.StudentID == studentId && s.ExamID == examId)
                                                .Join(_db.FEQuestions,
                                                sa => sa.QuestionID,
                                                q => q.FEQuestionId,
                                                (sa, q) => new
                                                {
                                                    StudentAnswer = sa.StudentAnswer,
                                                    studentId = sa.StudentID,
                                                    ExamQuestionID = sa.ExamQuestionID,
                                                    QuestionTypeID = q.QuestionTypeId,
                                                    QuestionID = sa.QuestionID,
                                                    QuestionMark = sa.QuestionMark
                                                }).Where(s => s.QuestionTypeID == 1)
                                                .Select(sa => new StudentAnswerDetail
                                                {
                                                    StudentAnswerContent = Int32.Parse(sa.StudentAnswer),
                                                    QuestionMark = (decimal)sa.QuestionMark
                                                }).ToListAsync();

            if (studentAnswersList.Count() == 0 || studentAnswersList == null)
            {
                return Tuple.Create(0, (decimal)0);
            }

            decimal totalMark = 0;

            foreach (var studentAs in studentAnswersList)
            {
                var answer = await _db.FEAnswers.Where(a => a.FEAnswerId == studentAs.StudentAnswerContent).FirstOrDefaultAsync();
                if (answer != null && answer.isCorrect == true)
                {
                    totalMark += studentAs.QuestionMark;
                }
            }
            totalMark = Math.Round(totalMark, 2);
            return Tuple.Create(1, totalMark);
        }
        public async Task<int> SaveStudentMark(decimal mark, int examId, int studentId)
        {
            var studentExamInfos = await _db.StudentExamInfos.FirstOrDefaultAsync(s => s.ExamId == examId
                                                                                       && s.StudentId == studentId);
            studentExamInfos.Mark = (float)mark;
            studentExamInfos.FinishAt=DateTime.Now;
            int rowAffected = _db.SaveChanges();
            return rowAffected;
        }
    }
}