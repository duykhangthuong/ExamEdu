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

        public async Task<List<QuestionAnswerForViewingResponse>> GetListQuestionAnswerByListQuestionIdForExamDetail(List<int> questionIdList, bool isFinalExam)
        {
            List<QuestionAnswerForViewingResponse> questionAnswerList = new List<QuestionAnswerForViewingResponse>();
            if (isFinalExam)
            {
                foreach (var questionId in questionIdList)
                {
                    FEQuestion question = await _dataContext.FEQuestions
                                                .Where(q => q.FEQuestionId == questionId && q.ApproveAt != null)
                                                .Select(q => new FEQuestion
                                                {
                                                    FEQuestionId = q.FEQuestionId,
                                                    QuestionContent = q.QuestionContent,
                                                    QuestionImageURL = q.QuestionImageURL,
                                                    Level = q.Level
                                                })
                                                .FirstOrDefaultAsync();
                    var questionAnswerResponse = _mapper.Map<QuestionAnswerForViewingResponse>(question);
                    questionAnswerResponse.Answers = new List<AnswerContentForViewingResponse>();
                    List<FEAnswer> answerList = await _dataContext.FEAnswers.Where(ans => ans.FEQuestionId == question.FEQuestionId).ToListAsync();
                    foreach (var answer in answerList)
                    {
                        questionAnswerResponse.Answers.Add(_mapper.Map<AnswerContentForViewingResponse>(answer));
                    }
                    questionAnswerList.Add(questionAnswerResponse);
                }
            }
            else
            {
                foreach (int questionId in questionIdList)
                {
                    Question question = await _dataContext.Questions
                                            .Where(q => q.QuestionId == questionId && q.ApproveAt != null)
                                            .Select(q => new Question
                                            {
                                                QuestionId = q.QuestionId,
                                                QuestionContent = q.QuestionContent,
                                                QuestionImageURL = q.QuestionImageURL,
                                                Level = q.Level
                                            })
                                            .FirstOrDefaultAsync();
                    var questionAnswerResponse = _mapper.Map<QuestionAnswerForViewingResponse>(question);
                    List<Answer> answerList = await _dataContext.Answers.Where(ans => ans.QuestionId == question.QuestionId).ToListAsync();
                    foreach (var answer in answerList)
                    {
                        questionAnswerResponse.Answers.Add(_mapper.Map<AnswerContentForViewingResponse>(answer));
                    }
                    questionAnswerList.Add(questionAnswerResponse);
                }
            }
            return questionAnswerList;
        }

        /// <summary>
        /// Create new request to add questions to bank
        /// </summary>
        /// <param name="addQuestionRequest"></param>
        /// <returns>Row inserted</returns>
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

        /// <summary>
        /// Get all the requests to add quesions to bank with pagination
        /// </summary>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public async Task<Tuple<int, IEnumerable<AddQuestionRequest>>> GetAllRequestAddQuestionBank(PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");

            IQueryable<AddQuestionRequest> requests = _dataContext.AddQuestionRequests;
            if (paginationParameter.SearchName != "")
            {
                requests = requests.Where(r => r.Requester.Fullname.ToLower().Contains(searchName.ToLower()));
            }

            IEnumerable<AddQuestionRequest> requestList = await requests
                                                .Select(r => new AddQuestionRequest
                                                {
                                                    AddQuestionRequestId = r.AddQuestionRequestId,
                                                    Requester = new Teacher
                                                    {
                                                        Fullname = r.Requester.Fullname
                                                    },
                                                    CreatedAt = r.CreatedAt,
                                                    Description = r.Description,
                                                    Questions = r.Questions.ToList(),
                                                    FEQuestions = r.FEQuestions.ToList(),
                                                    ApproverId = r.ApproverId,
                                                })
                                                .OrderByDescending(r => r.CreatedAt)
                                                .ToListAsync();

            return Tuple.Create(requestList.Count(), PaginationHelper.GetPage(requestList, paginationParameter));
        }

        /// <summary>
        /// Check whether the bank is final
        /// </summary>
        /// <param name="addQuestionRequestId"></param>
        /// <returns></returns>
        public bool IsFinalExamBank(int addQuestionRequestId)
        {
            if (_dataContext.FEQuestions.Where(q => q.AddQuestionRequestId == addQuestionRequestId).FirstOrDefault() != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a module's name by addQuestionRequestId
        /// </summary>
        /// <param name="addQuestionRequestId"></param>
        /// <param name="isFinalExam">To check for getting the module's name in fe bank or pt bank</param>
        /// <returns></returns>
        public async Task<string> GetModuleNameByAddQuestionRequestId(int addQuestionRequestId, bool isFinalExam)
        {
            string moduleName;
            if (isFinalExam)
            {
                moduleName = await _dataContext.FEQuestions.Where(q => q.AddQuestionRequestId == addQuestionRequestId && q.Module.ModuleName != null).Select(q => q.Module.ModuleName).FirstOrDefaultAsync();
            }
            else
            {
                moduleName = await _dataContext.Questions.Where(q => q.AddQuestionRequestId == addQuestionRequestId && q.Module.ModuleName != null).Select(q => q.Module.ModuleName).FirstOrDefaultAsync();
            }
            return moduleName;
        }

        /// <summary>
        /// Assign a teacher to approve the request add questions to bank
        /// </summary>
        /// <param name="addQuestionRequestId"></param>
        /// <param name="teacherId"></param>
        /// <returns></returns>
        public async Task<int> AssignTeacherToApproveRequest(int addQuestionRequestId, int teacherId)
        {
            int rowInserted = 0;
            var addQuestionRequest = await _dataContext.AddQuestionRequests.Where(s => s.AddQuestionRequestId == addQuestionRequestId).FirstOrDefaultAsync();
            if (addQuestionRequest != null)
            {
                if (addQuestionRequest.ApproverId != null)
                {
                    return -1;
                }
                addQuestionRequest.ApproverId = teacherId;
            }
            else
            {
                return -2;
            }
            _dataContext.AddQuestionRequests.Update(addQuestionRequest);
            rowInserted = await _dataContext.SaveChangesAsync();
            return rowInserted;
        }

        public async Task<bool> IsRequestExist(int addQuestionRequestId)
        {
            return await _dataContext.AddQuestionRequests.Where(s => s.AddQuestionRequestId == addQuestionRequestId).AnyAsync();
        }

        public async Task<AddQuestionRequest> GetRequestAddQuestionBankDetail(int addQuestionRequestId)
        {
            var request = await _dataContext.AddQuestionRequests
                                        .Where(r => r.AddQuestionRequestId == addQuestionRequestId)
                                        .Select(r => new AddQuestionRequest
                                        {
                                            AddQuestionRequestId = r.AddQuestionRequestId,
                                            Questions = r.Questions.Select(q => new Question
                                            {
                                                QuestionId = q.QuestionId,
                                                QuestionContent = q.QuestionContent,
                                                QuestionImageURL = q.QuestionImageURL,
                                                Module = new Module
                                                {
                                                    ModuleName = q.Module.ModuleName
                                                },
                                                Level = new Level
                                                {
                                                    LevelName = q.Level.LevelName
                                                },
                                                Answers = q.Answers.ToList(),
                                                AddQuestionRequestId = q.AddQuestionRequestId
                                            }).ToList(),
                                            FEQuestions = r.FEQuestions.Select(q => new FEQuestion
                                            {
                                                FEQuestionId = q.FEQuestionId,
                                                QuestionContent = q.QuestionContent,
                                                QuestionImageURL = q.QuestionImageURL,
                                                Module = new Module
                                                {
                                                    ModuleName = q.Module.ModuleName
                                                },
                                                Level = new Level
                                                {
                                                    LevelName = q.Level.LevelName
                                                },
                                                FEAnswers = q.FEAnswers.ToList(),
                                                AddQuestionRequestId = q.AddQuestionRequestId
                                            }).ToList()
                                        })
                                        .FirstOrDefaultAsync();
            return request;
        }

        public async Task<int> ApproveQuestion(QuestionToApproveInput input)
        {
            int rowInserted = 0;
            if (input.IsFinalExamBank)
            {
                var feQuestion = await _dataContext.FEQuestions.Where(q => q.FEQuestionId == input.QuestionId && q.ApproveAt == null).FirstOrDefaultAsync();
                if (feQuestion != null)
                {
                    feQuestion.isApproved = input.isApproved;
                    if (input.isApproved == true) { feQuestion.ApproveAt = DateTime.Now; }
                    // feQuestion.Comment=input.Comment;
                    _dataContext.FEQuestions.Update(feQuestion);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                var question = await _dataContext.Questions.Where(q => q.QuestionId == input.QuestionId && q.ApproveAt == null).FirstOrDefaultAsync();
                if (question != null)
                {
                    question.isApproved = input.isApproved;
                    if (input.isApproved == true) { question.ApproveAt = DateTime.Now; }
                    question.Comment = input.Comment;
                    _dataContext.Questions.Update(question);
                }
                else
                {
                    return -1;
                }
            }
            rowInserted = await _dataContext.SaveChangesAsync();
            return rowInserted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        public async Task<Tuple<int, IEnumerable<AddQuestionRequest>>> GetAllRequestAddQuestionByApproverId(int approverId, PaginationParameter paginationParameter)
        {
            string searchName = paginationParameter.SearchName;
            searchName = searchName.Replace(":*|", " ").Replace(":*", "");

            IQueryable<AddQuestionRequest> requests = _dataContext.AddQuestionRequests.Where(r => r.ApproverId == approverId);
            if (paginationParameter.SearchName != "")
            {
                requests = requests.Where(r => r.Requester.Fullname.ToUpper().Contains(searchName.ToUpper()));
            }

            IEnumerable<AddQuestionRequest> requestList = await requests
                                                .Select(r => new AddQuestionRequest
                                                {
                                                    AddQuestionRequestId = r.AddQuestionRequestId,
                                                    Requester = new Teacher
                                                    {
                                                        Fullname = r.Requester.Fullname
                                                    },
                                                    CreatedAt = r.CreatedAt,
                                                    Description = r.Description,
                                                    Questions = r.Questions.ToList(),
                                                    FEQuestions = r.FEQuestions.ToList()
                                                })
                                                .OrderByDescending(r => r.CreatedAt)
                                                .ToListAsync();

            return Tuple.Create(requestList.Count(), PaginationHelper.GetPage(requestList, paginationParameter));
        }

    }
}