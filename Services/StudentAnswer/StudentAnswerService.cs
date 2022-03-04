using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.StudentAnswerDTO;
using Microsoft.EntityFrameworkCore;

namespace ExamEdu.Services
{
    public class StudentAnswerService : IStudentAnswerService
    {
        private readonly DataContext _dataContext;
        public StudentAnswerService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<int> InsertStudentAnswers(List<StudentAnswer> answers)
        {
            foreach (var item in answers)
            {
                var checkStudent = _dataContext.Students.Any(t => t.StudentId == item.StudentId && t.DeactivatedAt == null);
                if (checkStudent is false)
                {
                    return -1;
                }
                var checkExamQuestion = _dataContext.ExamQuestions.Any(e => e.ExamQuestionId == item.ExamQuestionId);
                if (checkExamQuestion is false)
                {
                    return -2;
                }
            }
            _dataContext.StudentAnswers.AddRange(answers);
            int rowInserted = await _dataContext.SaveChangesAsync();
            return rowInserted;

        }
        public async Task<int> InsertFEStudentAnswers(List<StudentFEAnswer> answers)
        {
            foreach (var item in answers)
            {
                var checkStudent = _dataContext.Students.Any(t => t.StudentId == item.StudentId && t.DeactivatedAt == null);
                if (checkStudent is false)
                {
                    return -1;
                }
                var checkExamQuestion = _dataContext.Exam_FEQuestions.Any(e => e.ExamFEQuestionId == item.ExamFEQuestionId);
                if (checkExamQuestion is false)
                {
                    return -2;
                }
            }
            _dataContext.StudentFEAnswers.AddRange(answers);
            int rowInserted = await _dataContext.SaveChangesAsync();
            return rowInserted;

        }

        public async Task<List<StudentTextAnswerResponse>> GetStudentTextAnswer(int studentId, int examId)
        {
            var studentTextAnswersList = await _dataContext.StudentAnswers.Join(_dataContext.ExamQuestions,
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
                                                   .Join(_dataContext.Questions,
                                                   sa => sa.QuestionID,
                                                   q => q.QuestionId,
                                                   (sa, q) => new
                                                   {
                                                       StudentAnswer = sa.StudentAnswer,
                                                       StudentId = sa.StudentID,
                                                       ExamQuestionID = sa.ExamQuestionID,
                                                       QuestionTypeID = q.QuestionTypeId,
                                                       QuestionID = sa.QuestionID,
                                                       QuestionMark = sa.QuestionMark,
                                                       QuestionContent = q.QuestionContent,
                                                       QuestionImageURL = q.QuestionImageURL
                                                   }).Where(s => s.QuestionTypeID == 2)
                                                   .OrderBy(s=>s.ExamQuestionID)
                                                   .Select(sa => new StudentTextAnswerResponse
                                                   {
                                                       StudentId = sa.StudentId,
                                                       ExamQuestionId=sa.ExamQuestionID,
                                                       QuestionContent = sa.QuestionContent,
                                                       QuestionImageURL = sa.QuestionImageURL,
                                                       StudentAnswer = sa.StudentAnswer,
                                                       QuestionMark = sa.QuestionMark
                                                   })
                                                   .ToListAsync();
            if (studentTextAnswersList.Count() == 0 || studentTextAnswersList == null)
            {
                return null;
            }
            return studentTextAnswersList;
        }
        public async Task<List<StudentTextAnswerResponse>> GetStudentFETextAnswer(int studentId, int examId)
        {
            var studentTextAnswersList =await _dataContext.StudentFEAnswers.Join(_dataContext.Exam_FEQuestions,
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
                                                 .Join(_dataContext.FEQuestions,
                                                 sa => sa.QuestionID,
                                                 q => q.FEQuestionId,
                                                 (sa, q) => new
                                                 {
                                                     StudentAnswer = sa.StudentAnswer,
                                                     StudentId = sa.StudentID,
                                                     ExamQuestionID = sa.ExamQuestionID,
                                                     QuestionTypeID = q.QuestionTypeId,
                                                     QuestionID = sa.QuestionID,
                                                     QuestionMark = sa.QuestionMark,
                                                     QuestionContent = q.QuestionContent,
                                                     QuestionImageURL = q.QuestionImageURL
                                                 }).Where(s => s.QuestionTypeID == 2)
                                                    .OrderBy(s=>s.ExamQuestionID)
                                                    .Select(sa => new StudentTextAnswerResponse
                                                    {
                                                        StudentId = sa.StudentId,
                                                        ExamQuestionId=sa.ExamQuestionID,
                                                        QuestionContent = sa.QuestionContent,
                                                        QuestionImageURL = sa.QuestionImageURL,
                                                        StudentAnswer = sa.StudentAnswer,
                                                        QuestionMark = sa.QuestionMark

                                                    })
                                                    .ToListAsync();
            if (studentTextAnswersList.Count() == 0 || studentTextAnswersList == null)
            {
                return null;
            }
            return studentTextAnswersList;
        }
    }
}