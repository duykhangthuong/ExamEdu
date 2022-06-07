using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Services;
using examedu.Services.Classes;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ClassModuleDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace examedu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClassModuleController : ControllerBase
    {
        private readonly IClassModuleService _classModuleService;
        private readonly ITeacherService _teacherService;
        private readonly IModuleService _moduleService;
        private readonly IClassService _classService;
        private readonly IMapper _mapper;

        public ClassModuleController(IClassModuleService classModuleService,
                                     IMapper mapper,
                                     ITeacherService teacherService,
                                     IModuleService moduleService,
                                     IClassService classService)
        {
            _classModuleService = classModuleService;
            _mapper = mapper;
            _teacherService = teacherService;
            _moduleService = moduleService;
            _classService = classService;
        }

        //Get ClassModule from ClassModuleId
        [HttpGet("{classModuleId:int}")]
        public async Task<IActionResult> GetClassModuleInfo(int classModuleId)
        {
            ClassModule classModule = await _classModuleService.GetClassModuleInfo(classModuleId);
            if (classModule == null)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            ClassModuleResponse classModuleResponse = _mapper.Map<ClassModuleResponse>(classModule);
            return Ok(classModuleResponse);
        }

        [HttpGet("{teacherId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetClassModules(int teacherId, int moduleId, [FromQuery] PaginationParameter paginationParameter)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher does not exist"));
            }
            //If module does not exist return bad request
            if (await _moduleService.getModuleByID(moduleId) == null)
            {
                return BadRequest(new ResponseDTO(400, "Module does not exist"));
            }

            var classModule = await _classModuleService.GetClassModules(teacherId, moduleId, paginationParameter);
            if (classModule.Item1 == 0)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            var classModuleResponse = _mapper.Map<IEnumerable<ClassModuleClassResponse>>(classModule.Item2);

            return Ok(new PaginationResponse<IEnumerable<ClassModuleClassResponse>>(classModule.Item1, classModuleResponse));
        }
        [HttpGet("student/{classId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetClassModulesWithStudent(int classId, int moduleId)
        {
            //If class does not exist return bad request
            if (await _classService.IsClassExist(classId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Class does not exist"));
            }
            //If module does not exist return bad request
            if (_moduleService.IsModuleExist(moduleId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Module does not exist"));
            }

            var classModule = await _classModuleService.GetClassModuleAndStudent(classId, moduleId);
            if (classModule == null)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            var classModuleStudentResponse = _mapper.Map<ClassModuleStudentResponse>(classModule);

            return Ok(classModuleStudentResponse);
        }
        [HttpPut("{classModuleId:int}")]
        public async Task<IActionResult> UpdateStudentListAndTeacher(int classModuleId,
                                                                     [FromBody] TeacherStudentInput teacherStudentInput)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherStudentInput.TeacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher does not exist"));
            }

            //If classModule does not exist return bad request
            if (await _classModuleService.IsClassModuleExist(classModuleId) == false)
            {
                return BadRequest(new ResponseDTO(400, "ClassModule does not exist"));
            }

            //If studentIds is empty return bad request
            if (teacherStudentInput.StudentIds.Count == 0)
            {
                return BadRequest(new ResponseDTO(400, "Student list input is empty"));
            }
            var result = await _classModuleService.UpdateStudentListAndTeacher(classModuleId, teacherStudentInput.TeacherId, teacherStudentInput.StudentIds);
            return Ok(new ResponseDTO(200, "Class module updated"));
        }
    }
}