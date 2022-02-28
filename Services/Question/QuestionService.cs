using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.DTO.QuestionDTO;
using ExamEdu.DB;
using Microsoft.EntityFrameworkCore;

namespace examedu.Services.Question
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
        public async Task<List<QuestionResponse>> GetQuestionByModuleLevel(int moduleID, int levelID, bool isFinalExam)
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
    }
}