using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Services;
using examedu.DTO.StudentDTO;
using examedu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace examedu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly IModuleService _moduleService;
        private readonly IMapper _mapper;


        public StudentController(IStudentService studentService, ITeacherService teacherService, IModuleService moduleService, IMapper mapper)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _moduleService = moduleService;
            _mapper = mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="moduleId"></param>
        /// <returns>400 student or moduleID not exist / 404 Student dont have exam</returns>
        [HttpGet("markReport/{studentID:int}/{moduleID:int}")]
        public async Task<ActionResult<List<ModuleMarkDTO>>> MarkReport(int studentId, int moduleId)
        {
            List<ModuleMarkDTO> listResult = await _studentService.getModuleMark(studentId, moduleId);
            if (listResult == null)
            {
                return BadRequest(new ResponseDTO(400, "StudentID or ModuleID not exist"));
            }
            if (listResult.Count == 0)
            {
                return NotFound(new ResponseDTO(404, "Student maynot have exam in this module"));
            }
            return Ok(listResult);
        }

        [HttpGet("{teacherId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetStudents(int teacherId, int moduleId, [FromQuery] PaginationParameter paginationParameter)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher does not exist"));
            }
            //If module does not exist return bad request
            if (await _moduleService.getModuleByID(moduleId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Module does not exist"));
            }

            (int totalRecord, IEnumerable<Student> students) = await _studentService.GetStudents(teacherId, moduleId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No student found"));
            }
            IEnumerable<StudentResponse> studentsResponses = _mapper.Map<IEnumerable<StudentResponse>>(students);
            return Ok(new PaginationResponse<IEnumerable<StudentResponse>>(totalRecord, studentsResponses));
        }

        [HttpGet("{classModuleId:int}")]
        public async Task<IActionResult> GetStudents(int classModuleId, [FromQuery] PaginationParameter paginationParameter)
        {

            (int totalRecord, IEnumerable<Student> students) = await _studentService.GetStudents(classModuleId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No student found"));
            }
            IEnumerable<StudentResponse> studentsResponses = _mapper.Map<IEnumerable<StudentResponse>>(students);
            return Ok(new PaginationResponse<IEnumerable<StudentResponse>>(totalRecord, studentsResponses));
        }
    }
}