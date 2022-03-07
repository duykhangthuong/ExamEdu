using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.TeacherDTO;
using ExamEdu.DB.Models;

namespace BackEnd.DTO.Profiles
{
    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
            CreateMap<Teacher, TeacherResponse>();
        }
    }
}