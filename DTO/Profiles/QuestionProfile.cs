using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamQuestionsDTO;
using BackEnd.DTO.QuestionDTO;
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
            CreateMap<Question, QuestionAnswerResponse>();
            CreateMap<FEQuestion, QuestionAnswerResponse>();
            CreateMap<Answer, AnswerContentResponse>();
            CreateMap<FEAnswer, AnswerContentResponse>();
            CreateMap<RequestAddQuestionInput, AddQuestionRequest>()
                    .ForMember(dest => dest.Questions, opt =>
                    {
                        opt.Condition(src => src.isFinalExam == false);
                        opt.MapFrom(src => src.Questions);
                    })
                    .ForMember(dest => dest.FEQuestions, opt =>
                    {
                        opt.Condition(src => src.isFinalExam == true);
                        opt.MapFrom(src => src.Questions);
                    });
            CreateMap<QuestionInput, Question>();
            CreateMap<QuestionInput, FEQuestion>()
                    .ForMember(dest => dest.FEAnswers, opt => opt.MapFrom(src => src.Answers));
            CreateMap<AnswerInput, Answer>();
            CreateMap<AnswerInput, FEAnswer>();
            CreateMap<AddQuestionRequest, RequestAddQuestionResponse>()
                    .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => src.Requester.Fullname))
                    // .ForMember(dest => dest.ModuleName, opt =>
                    //      opt.MapFrom(src =>
                    //         src.Questions.Where(q=>q.AddQuestionRequestId==src.AddQuestionRequestId).FirstOrDefault()!=null
                    //          src.Questions.Select(q => q.Module.ModuleName).First() : src.FEQuestions.Select(q => q.Module.ModuleName).First()
                    // ))
                    .ForMember(dest => dest.NumberOfQuestion, opt =>
                           opt.MapFrom(src =>
                            src.Questions.Where(q=>q.AddQuestionRequestId==src.AddQuestionRequestId).FirstOrDefault()!=null
                            ? src.Questions.Count() : src.FEQuestions.Count())
                    )
                    .ForMember(dest => dest.IsAssigned, opt => opt.MapFrom(src => src.ApproverId != null ? true : false));

        }
    }
}