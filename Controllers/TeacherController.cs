using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DTO.PaginationDTO;
using BackEnd.Services;
using ExamEdu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ModuleDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ExamEdu.DTO.ClassModuleDTO;
using BackEnd.DTO.TeacherDTO;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IClassModuleService _classModuleService;
        private readonly IModuleService _moduleService;
        private readonly IMapper _mapper;

        public TeacherController(ITeacherService teacherService, IClassModuleService classModuleService, IModuleService moduleService, IMapper mapper)
        {
            _teacherService = teacherService;
            _classModuleService = classModuleService;
            _moduleService = moduleService;
            _mapper = mapper;
        }

        [HttpGet("ClassModule/{teacherId:int}")]
        public async Task<IActionResult> GetClassModuleOfTeacher(int teacherId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Module> modules) = await _moduleService.getModulesWithClassModule(paginationParameter, teacherId);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            IEnumerable<ModuleTeacherResponse> classModuleResponses = _mapper.Map<IEnumerable<ModuleTeacherResponse>>(modules);

            return Ok(new PaginationResponse<IEnumerable<ModuleTeacherResponse>>(totalRecord, classModuleResponses));
        }

        [HttpGet("idName")]
        public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAllTeacherIdAndName()
        {
            var teachers = await _teacherService.GetAllTeacherIdAndName();
            var teachersResponse = _mapper.Map<IEnumerable<TeacherResponse>>(teachers);
            return Ok(teachersResponse);
        }

        //http get teacherId parameter
        [HttpGet("check/{teacherId:int}")]
        //return 200 status code if teacher is IsHeadOfDepartment
        public async Task<ActionResult<bool>> IsHeadOfDepartment(int teacherId)
        {

            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return NotFound(new ResponseDTO(404, "Teacher Not Found"));
            }
            return Ok(await _teacherService.IsHeadOfDepartment(teacherId));
        }
    }
}