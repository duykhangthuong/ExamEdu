using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.DTO.CheatDTO;
using ExamEdu.DB.Models;

namespace examedu.DTO.Profiles
{
    public class CheatTypeProfile :  Profile
    {
        public CheatTypeProfile()
        {
            CreateMap<CheatingType, CheatTypeResponse>();
            CreateMap<StudentCheatingInput, StudentCheating>();
        }
    }
}