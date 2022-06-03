using AutoMapper;
using ExamEdu.DB.Models;
using ExamEdu.DTO.AcademicDepartmentDTO;

namespace ExamEdu.DTO.Profiles
{
    public class AcademicDepartmentProfile : Profile
    {
        public AcademicDepartmentProfile()
        {
            CreateMap<AcademicDepartment, AcademicDepartmentResponse>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AcademicDepartmentId));
        }
    }
}