using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.DTO.ExamQuestionsDTO;
using BackEnd.DTO.QuestionDTO;
using examedu.DTO.QuestionDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace examedu.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionResponse>> getQuestionByModuleLevel(int moduleID, int levelID, bool isFinalExam);
        Task<List<QuestionAnswerResponse>> GetListQuestionAnswerByListQuestionId(List<int> questionIdList, int examId, int examCode, bool isFinalExam);
        Task<List<QuestionAnswerForViewingResponse>> GetListQuestionAnswerByListQuestionIdForExamDetail(List<int> questionIdList, bool isFinalExam);
        Task<int> InsertNewRequestAddQuestions(AddQuestionRequest addQuestionRequest);
        Task<Tuple<int, IEnumerable<AddQuestionRequest>>> GetAllRequestAddQuestionBank(PaginationParameter paginationParameter);
        bool IsFinalExamBank(int addQuestionRequestId);
        Task<string> GetModuleNameByAddQuestionRequestId(int addQuestionRequestId, bool isFinalExam);
        Task<int> AssignTeacherToApproveRequest(int addQuestionRequestId, int teacherId);
        Task<bool> IsRequestExist(int addQuestionRequestId);
        Task<AddQuestionRequest> GetRequestAddQuestionBankDetail(int addQuestionRequestId);
        Task<int> ApproveQuestion(QuestionToApproveInput input);
        Task<Tuple<int, IEnumerable<AddQuestionRequest>>> GetAllRequestAddQuestionByApproverId(int approverId, PaginationParameter paginationParameter);
    }
}