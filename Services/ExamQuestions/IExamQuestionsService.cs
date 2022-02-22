using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services.ExamQuestions
{
    public interface IExamQuestionsService
    {
        Task<List<int>> GetListQuestionIdByExamIdAndExamCode(int examId, int examCode, bool isFinalExam);
        Task<int> GetRandomExamCodeByExamId(int examId, bool isFinalExam);
    }
}