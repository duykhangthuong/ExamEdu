using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services.ExamQuestions
{
    public class ExamQuestionsService : IExamQuestionsService
    {
        private readonly DataContext _dataContext;
        public ExamQuestionsService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        /// <summary>
        /// Get a list of question id by exam id and exam code
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="examCode"></param>
        /// <param name="isFinalExam"></param>
        /// <returns></returns>
        public async Task<List<int>> GetListQuestionIdByExamIdAndExamCode(int examId, int examCode, bool isFinalExam)
        {
            if (isFinalExam)
            {
                return await _dataContext.Exam_FEQuestions.Where(e => e.ExamId == examId && e.ExamCode == examCode).Select(e => e.FEQuestionId).ToListAsync();
            }
            else
            {
                return await _dataContext.ExamQuestions.Where(e => e.ExamId == examId && e.ExamCode == examCode).Select(e => e.QuestionId).ToListAsync();
            }
        }

        public async Task<int> GetRandomExamCodeByExamId(int examId, bool isFinalExam)
        {
            Random rand = new Random();
            int toSkip;
            if (isFinalExam)
            {
                var examCodeList = _dataContext.Exam_FEQuestions.Where(e => e.ExamId == examId).Select(e => e.ExamCode);
                toSkip = rand.Next(1, examCodeList.Count());
                //return await _dataContext.Exam_FEQuestions.Where(e => e.ExamId == examId).OrderBy(r => Guid.NewGuid()).Select(e => e.ExamCode).FirstOrDefaultAsync();
                return await examCodeList.Skip(toSkip).Take(1).FirstOrDefaultAsync();
            }
            else
            {
                var examCodeList = _dataContext.ExamQuestions.Where(e => e.ExamId == examId).Select(e => e.ExamCode);
                toSkip = rand.Next(1, examCodeList.Count());
                //return await _dataContext.ExamQuestions.Where(e => e.ExamId == examId).OrderBy(r => Guid.NewGuid()).Select(e => e.ExamCode).FirstOrDefaultAsync();
                return await examCodeList.Skip(toSkip).Take(1).FirstOrDefaultAsync();
            }
        }
    }
}