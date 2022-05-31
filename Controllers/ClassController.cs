using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Services;
using examedu.Services.Classes;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace examedu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {

        private readonly IClassService _classService;
        private readonly IMapper _mapper;
        private readonly IModuleService _moduleService;
        private readonly ITeacherService _teacherService;

        public ClassController(IClassService classService,
                               IMapper mapper,
                               IModuleService moduleService,
                               ITeacherService teacherService)
        {
            _classService = classService;
            _mapper = mapper;
            _moduleService = moduleService;
            _teacherService = teacherService;
        }


        [HttpGet("{teacherId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetClasses(int teacherId, int moduleId, [FromQuery] PaginationParameter paginationParameter)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher not found"));
            }
            //If module does not exist return bad request
            if (await _moduleService.getModuleByID(moduleId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Module not found"));
            }

            var classes = await _classService.GetClasses(teacherId, moduleId, paginationParameter);

            if (classes.Item1 == 0)
            {
                return NotFound(new ResponseDTO(404, "No class found"));
            }
            //Map classes to ClassNameResponse
            var classesResponse = _mapper.Map<IEnumerable<ClassNameResponse>>(classes.Item2);

            //Return the classes in a pagination response
            return Ok(new PaginationResponse<IEnumerable<ClassNameResponse>>(classes.Item1, classesResponse));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses([FromQuery] PaginationParameter paginationParameter)
        {
            var classes = await _classService.GetAllClasses(paginationParameter);

            if (classes.Item1 == 0)
            {
                return NotFound(new ResponseDTO(404, "No class found"));
            }
            //Map classes to ClassNameResponse
            var classesResponse = _mapper.Map<IEnumerable<ClassNameResponse>>(classes.Item2);

            //Return the classes in a pagination response
            return Ok(new PaginationResponse<IEnumerable<ClassNameResponse>>(classes.Item1, classesResponse));
        }
    }
}