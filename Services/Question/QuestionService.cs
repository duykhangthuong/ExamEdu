using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamQuestionsDTO;
using BackEnd.DTO.QuestionDTO;
using examedu.DTO.QuestionDTO;
using ExamEdu.DB;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Helper;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public QuestionService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        /// <summary>
        /// get Question by module, level, and isFinalExam
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="levelID"></param>
        /// <param name="isFinalExam"></param>
        /// <returns></returns>
        public async Task<List<QuestionResponse>> getQuestionByModuleLevel(int moduleID, int levelID, bool isFinalExam)
        {
            List<QuestionResponse> listResponse = new List<QuestionResponse>();
            if (isFinalExam)
            {
                var QuestionListFromDB = await _dataContext.FEQuestions.Where(q =>
                    q.ModuleId == moduleID && q.LevelId == levelID && q.ApproveAt != null).ToListAsync();
                foreach (var question in QuestionListFromDB)
                {
                    //return await _db.Answers.Where(a => a.QuestionId == questionID).ToListAsync();
                    QuestionResponse questionResponse = _mapper.Map<QuestionResponse>(question);
                    questionResponse.Answers = new List<AnswerResponse>();
                    var Answers = await _dataContext.FEAnswers.Where(a => a.FEQuestionId == question.FEQuestionId).ToListAsync();
                    foreach (var answer in Answers)
                    {
                        questionResponse.Answers.Add(_mapper.Map<AnswerResponse>(answer));
                    }
                    listResponse.Add(questionResponse);
                }
            }
            else
            {
                var QuestionListFromDB = await _dataContext.Questions.Where(q =>
                    q.ModuleId == moduleID && q.LevelId == levelID && q.ApproveAt != null).ToListAsync();
                foreach (var question in QuestionListFromDB)
                {
                    QuestionResponse questionResponse = _mapper.Map<QuestionResponse>(question);
                    var Answers = await _dataContext.Answers.Where(a => a.QuestionId == question.QuestionId).ToListAsync();
                    foreach (var answer in Answers)
                    {
                        questionResponse.Answers.Add(_mapper.Map<AnswerResponse>(answer));
                    }
                    listResponse.Add(questionResponse);
                }
            }
            return listResponse;
        }

        /// <summary>
        /// Get a list of questions and answers by a list of question id
        /// </summary>
        /// <param name="questionIdList">list of question id</param>
        /// <param name="examId">use to get ExamQuestionId</param>
        /// <param name="examCode">use to get ExamQuestionId</param>
        /// <param name="isFinalExam"></param>
        /// <returns></returns>
        public async Task<List<QuestionAnswerResponse>> GetListQuestionAnswerByListQuestionId(List<int> questionIdList, int examId, int examCode, bool isFinalExam)
        {
            List<QuestionAnswerResponse> questionAnswerList = new List<QuestionAnswerResponse>();
            if (isFinalExam)
            {
                foreach (var questionId in questionIdList)
                {
                    FEQuestion question = await _dataContext.FEQuestions.Where(q => q.FEQuestionId == questionId && q.ApproveAt != null).FirstOrDefaultAsync();
                    var questionAnswerResponse = _mapper.Map<QuestionAnswerResponse>(question);
                    List<FEAnswer> answerList = await _dataContext.FEAnswers.Where(ans => ans.FEAnswerId == question.FEQuestionId).ToListAsync();
                    foreach (var answer in answerList)
                    {
                        questionAnswerResponse.Answers.Add(_mapper.Map<AnswerContentResponse>(answer));
                    }
                    questionAnswerResponse.ExamQuestionId = await _dataContext.Exam_FEQuestions.Where(e => e.ExamId == examId && e.ExamCode == examCode && e.FEQuestionId == questionId).Select(e => e.ExamFEQuestionId).FirstOrDefaultAsync();
                    questionAnswerList.Add(questionAnswerResponse);
                }
            }
            else
            {
                foreach (int questionId in questionIdList)
                {
                    Question question = await _dataContext.Questions.Where(q => q.QuestionId == questionId && q.ApproveAt != null).FirstOrDefaultAsync();
                    var questionAnswerResponse = _mapper.Map<QuestionAnswerResponse>(question);
                    List<Answer> answerList = await _dataContext.Answers.Where(ans => ans.QuestionId == question.QuestionId).ToListAsync();
                    foreach (var answer in answerList)
                    {
                        questionAnswerResponse.Answers.Add(_mapper.Map<AnswerContentResponse>(answer));
                    }
                    questionAnswerResponse.ExamQuestionId = await _dataContext.ExamQuestions.Where(e => e.ExamId == examId && e.ExamCode == examCode && e.QuestionId == questionId).Select(e => e.ExamQuestionId).FirstOrDefaultAsync();
                    questionAnswerList.Add(questionAnswerResponse);
                }
            }
            return questionAnswerList;
        }

        public async Task<int> InsertNewRequestAddQuestions(AddQuestionRequest addQuestionRequest)
        {
            using (var dbContextTransaction = _dataContext.Database.BeginTransaction())
            {
                try
                {
                    int rowInserted = 0;
                    _dataContext.AddQuestionRequests.Add(addQuestionRequest);
                    rowInserted = await _dataContext.SaveChangesAsync();
                    dbContextTransaction.Commit();
                    return rowInserted;
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return 0;
                }
            }
        }

        public async Task<Tuple<int, IEnumerable<AddQuestionRequest>>> GetAllRequestAddQuestionBank(PaginationParameter paginationParameter)
        {
            IQueryable<AddQuestionRequest> requests = _dataContext.AddQuestionRequests;
            if (paginationParameter.SearchName != "")
            {
                requests = requests.Where(r => EF.Functions.ToTsVector("simple", EF.Functions.Unaccent(r.Requester.Fullname.ToLower()))
                      .Matches(EF.Functions.ToTsQuery("simple", EF.Functions.Unaccent(paginationParameter.SearchName.ToLower()))));
            }

            IEnumerable<AddQuestionRequest> requestList = await requests
                                                .GetCount(out var totalRecord)
                                                .GetPage(paginationParameter)
                                                .Select(r => new AddQuestionRequest
                                                {
                                                    AddQuestionRequestId = r.AddQuestionRequestId,
                                                    Requester = new Teacher
                                                    {
                                                        Fullname = r.Requester.Fullname
                                                    },
                                                    CreatedAt = r.CreatedAt,
                                                    Description = r.Description,
                                                    Questions = r.Questions,
                                                    FEQuestions = r.FEQuestions.ToList(),
                                                    ApproverId = r.ApproverId,
                                                })
                                                .OrderByDescending(r => r.CreatedAt)
                                                .ToListAsync();

            return Tuple.Create(totalRecord, requestList);
        }

        public bool IsFinalExamBank(int addQuestionRequestId)
        {
            if (_dataContext.FEQuestions.Where(q => q.AddQuestionRequestId == addQuestionRequestId).First() != null)
            {
                return true;
            }
            return false;
        }
    }
}