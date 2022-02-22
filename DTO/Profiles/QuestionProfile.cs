using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamQuestionsDTO;
using examedu.DTO.QuestionDTO;
using ExamEdu.DB.Models;

namespace examedu.DTO.Profiles
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionResponse>();
            CreateMap<FEQuestion, QuestionResponse>().ForMember(q => q.QuestionId, s => s.MapFrom(s => s.FEQuestionId));
            CreateMap<Answer, AnswerResponse>();
            CreateMap<FEAnswer, AnswerResponse>();
            CreateMap<Question,QuestionAnswerResponse>();
            CreateMap<FEQuestion,QuestionAnswerResponse>();
            CreateMap<Answer,AnswerContentResponse>();
            CreateMap<FEAnswer,AnswerContentResponse>();
        }
    }
}