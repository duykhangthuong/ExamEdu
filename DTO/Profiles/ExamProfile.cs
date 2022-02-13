using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamQuestionsDTO;
using examedu.DTO.ExamDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ExamDTO;

namespace ExamEdu.DTO.Profiles
{
    public class ExamProfile : Profile
    {
        public ExamProfile()
        {
            CreateMap<Exam, ExamScheduleResponse>();
            CreateMap<CreateExamByHandInput, Exam_FEQuestion>();
            CreateMap<CreateExamByHandInput, ExamQuestion>();
            CreateMap<Exam, ExamResponse>();
            CreateMap<Exam, ExamQuestionsResponse>().ForMember(eqr=>eqr.ModuleName,s=>s.MapFrom(s=>s.Module.ModuleName));
            CreateMap<Exam, ProgressExamResponse>();
        }

    }
}