using AutoMapper;
using ExamEdu.DTO.ClassModuleDTO;
using ExamEdu.DTO.ModuleDTO;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DB.Models;

namespace ExamEdu.DTO.Profiles
{
    public class ClassModuleProfile : Profile
    {
        public ClassModuleProfile()
        {
            CreateMap<ClassModule, ClassModuleResponse>();
            CreateMap<Module, ModuleResponse>();
            CreateMap<Class, ClassNameResponse>();
        }
    }
}