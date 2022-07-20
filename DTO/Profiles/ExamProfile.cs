using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamDTO;
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
            CreateMap<Exam, ExamScheduleResponse>().ForMember(esr => esr.ModuleCode, s => s.MapFrom(s => s.Module.ModuleCode));
            CreateMap<CreateExamByHandInput, Exam_FEQuestion>();
            CreateMap<CreateExamByHandInput, ExamQuestion>();
            CreateMap<Exam, ExamResponse>();
            CreateMap<Exam, ExamQuestionsResponse>()
                .ForMember(eqr => eqr.ModuleCode, s => s.MapFrom(s => s.Module.ModuleCode));
            CreateMap<Exam, ProgressExamResponse>();

            //Mapping for create exam info
            CreateMap<CreateExamInfoInput, Exam>()
                .ForMember(e => e.CreatedAt, s => s.MapFrom(s => DateTime.Now))
                .ForMember(e => e.IsCancelled, s => s.MapFrom(s => false))
                .ForMember(e => e.StudentExamInfos, s => s.MapFrom(s => s.StudentIds.Select(id => new StudentExamInfo() { StudentId = id })));

            //Mapping for update exam info input
            CreateMap<UpdateExamInfoInput, Exam>();
            //Mapping for update exam info response
            CreateMap<Exam, UpdateExamInfoResponse>();
            CreateMap<Exam, GetAllExam>();
            CreateMap<Exam, ExamDetailResponse>()
                .ForMember(dest => dest.ModuleCode, opt => opt.MapFrom(s => s.Module.ModuleCode))
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(s => s.Module.ModuleName))
                .ForMember(dest => dest.ProctorEmail, opt => opt.MapFrom(s => s.Proctor.Email))
                .ForMember(dest => dest.ProctorFullName, opt => opt.MapFrom(s => s.Proctor.Fullname))
                .ForMember(dest => dest.SupervisorEmail, opt => opt.MapFrom(s => s.Supervisor.Email));
            CreateMap<Exam,ExamProctorResponse>()
                .ForMember(dest => dest.ModuleCode, opt => opt.MapFrom(s => s.Module.ModuleCode))
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(s => s.Module.ModuleName))
                .ForMember(dest => dest.SupervisorEmail, opt => opt.MapFrom(s => s.Supervisor.Email));
        }

    }
}