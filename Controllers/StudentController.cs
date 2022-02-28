using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.StudentDTO;
using examedu.Services;
using ExamEdu.DTO;
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
        
        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
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
            List<ModuleMarkDTO> listResult = await _studentService.GetModuleMark(studentId, moduleId);
            if(listResult == null)
            {
                return BadRequest(new ResponseDTO(400, "StudentID or ModuleID not exist"));
            }
            if(listResult.Count == 0)
            {
                return NotFound(new ResponseDTO(404, "Student maynot have exam in this module"));
            }
            return Ok(listResult);
        }
    }
}