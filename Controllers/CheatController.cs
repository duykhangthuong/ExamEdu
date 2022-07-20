using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.DTO.CheatDTO;
using examedu.Services;
using examedu.Services.Cheat;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Mvc;

namespace examedu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheatController : ControllerBase
    {
        private readonly ICheatService _cheatService;
        private readonly IMapper _mapper;
        private readonly IStudentService _studentService;
        private readonly IExamService _examService;

        public CheatController(ICheatService cheatService, IMapper mapper, IStudentService studentService, IExamService examService)
        {
            _cheatService = cheatService;
            _mapper = mapper;
            _studentService = studentService;
            _examService = examService;
        }

        [HttpGet("CheatingTypeList")]
        public async Task<IActionResult> CheatingTypeList()
        {
            List<CheatingType> cheatingTypeList = await _cheatService.GetCheatingTypeList();
            if (cheatingTypeList == null || cheatingTypeList.Count == 0)
            {
                return BadRequest(new ResponseDTO(400, "CheatingTypeList does not exist"));
            }
            List<CheatTypeResponse> cheatTypeResponseList = new List<CheatTypeResponse>();
            foreach (var item in cheatingTypeList)
            {
                cheatTypeResponseList.Add(_mapper.Map<CheatTypeResponse>(item));
            }
            return Ok(cheatTypeResponseList);
        }
        [HttpPost("StudentCheating")]
        public async Task<IActionResult> StudentCheating([FromBody] StudentCheatingInput studentCheatingInput)
        {
            Student student = await _studentService.GetStudentByEmail(studentCheatingInput.StudentEmail);
            Exam exam = await _examService.getExamById(studentCheatingInput.ExamId);
            if (student == null || exam == null)
            {
                return BadRequest(new ResponseDTO(400, "Student or Exam does not exist"));
            }
            StudentCheating studentCheating = _mapper.Map<StudentCheating>(studentCheatingInput);
            studentCheating.StudentId = student.StudentId;
            int result = await _cheatService.CreateStudentCheating(studentCheating);
            if (result == 0)
            {
                return BadRequest(new ResponseDTO(400, "Some error occurred"));
            }
            return Ok(new ResponseDTO(200, "StudentCheating created"));
        }
    }
}