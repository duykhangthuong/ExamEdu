using System.Linq;
using AutoMapper;
using BackEnd.DTO.ClassDTO;
using BackEnd.DTO.ClassModuleDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.ClassModuleDTO;

namespace ExamEdu.DTO.Profiles
{
    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<ModuleTeacherStudentInput, ClassModule>()
                .ForMember(dest => dest.Class_Module_Students, opt => opt.MapFrom(src => src.StudentIds.Select(id => new Class_Module_Student() { StudentId = id })));
            CreateMap<CreateClassInput, Class>()
                .ForMember(dest => dest.ClassModules, opt => opt.MapFrom(src => src.ModuleTeacherStudentIds));
            CreateMap<ClassModule, ClassModuleResponse2>();
            CreateMap<Class,ClassResponse>();
            CreateMap<ClassBasicInforInput,Class>(); 
        }
    }
}
