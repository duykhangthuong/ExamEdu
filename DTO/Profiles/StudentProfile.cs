using AutoMapper;
using BackEnd.DTO.StudentDTO;
using examedu.DTO.StudentDTO;
using ExamEdu.DB.Models;

namespace ExamEdu.DTO.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<Student, StudentResponse>();
            CreateMap<Student, StudentInforResponse>();
        }
    }
}