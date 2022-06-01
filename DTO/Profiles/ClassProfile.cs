using AutoMapper;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.ClassModuleDTO;

namespace ExamEdu.DTO.Profiles
{
    public class ClassProfile : Profile
    {
        public ClassProfile()
        {
            CreateMap<ClassModule, ClassModuleResponse2>();
            CreateMap<Class,ClassResponse>();
            
        }
    }
}
