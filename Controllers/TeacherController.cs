using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DTO.PaginationDTO;
using BackEnd.Services;
using ExamEdu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ExamEdu.DTO.ClassModuleDTO;


namespace ExamEdu.Controllers
{
    [ApiController]
    // [Authorize]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IClassModuleService _classModuleService;
        private readonly IMapper _mapper;

        public TeacherController(ITeacherService teacherService, IClassModuleService classModuleService, IMapper mapper)
        {
            _teacherService = teacherService;
            _classModuleService = classModuleService;
            _mapper = mapper;
        }

        [HttpGet("ClassModule/{teacherId:int}")]
        public async Task<IActionResult> GetClassModuleOfTeacher(int teacherId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<ClassModule> classModules) = await _classModuleService.GetClassModuleByTeacherId(teacherId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            IEnumerable<ClassModuleResponse> classModuleResponses = _mapper.Map<IEnumerable<ClassModuleResponse>>(classModules);

            return Ok(new PaginationResponse<IEnumerable<ClassModuleResponse>>(totalRecord, classModuleResponses));
        }
    }
}