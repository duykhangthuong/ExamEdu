using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExamEdu.DB.Models;
using ExamEdu.DTO.ModuleDTO;
using ExamEdu.DTO.ClassModuleDTO;
using ExamEdu.DTO.ClassDTO;

namespace ExamEdu.DTO.Profiles
{
    public class ModuleProfiles : Profile
    {
        public ModuleProfiles()
        {
            CreateMap<Module, ModuleResponse>();
            CreateMap<Module, ModuleInformationResponse>();

            CreateMap<ModuleInput, Module>();

            //Map for ClassModuleResponse
            CreateMap<Module, ModuleTeacherResponse>();
            CreateMap<ClassModule, ClassModuleWithClassInfoResponse>();
            CreateMap<Class, ClassNameResponse>();
        }
    }
}