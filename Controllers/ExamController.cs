using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.DTO.ExamDTO;
using examedu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ExamDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;
        public ExamController(IExamService examService, IMapper mapper, IStudentService studentService)
        {
            _examService = examService;
            _mapper = mapper;
            _studentService = studentService;
        }
        [HttpGet("{studentId:int}")]
        public async Task<IActionResult> GetExamScheduleByStudentId(int studentId, [FromQuery] PaginationParameter paginationParameter)
        {
            bool isStudentExist = _studentService.CheckStudentExist(studentId);
            if (isStudentExist == false)
            {
                return NotFound(new ResponseDTO(404, "Student not found"));
            }
            (int totalRecord, IEnumerable<Exam> examSchedules) = await _examService.getExamByStudentId(studentId, paginationParameter);

            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Student doesn't have exam schedules"));
            }
            IEnumerable<ExamScheduleResponse> examScheduleResponses = _mapper.Map<IEnumerable<ExamScheduleResponse>>(examSchedules);
            return Ok(new PaginationResponse<IEnumerable<ExamScheduleResponse>>(totalRecord, examScheduleResponses));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>201: success  / 400 error when insert</returns>
        [HttpPost("byHand")]
        public async Task<IActionResult> CreateExamByHand(CreateExamByHandInput input)
        {
            if (await _examService.getExamById(input.ExamId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Exam not existed"));
            }
            int status = await _examService.CreateExamPaperByHand(input);
            if (status == 1)
            {
                return Created(nameof(CreateExamByHand), new ResponseDTO(201, "Successfully inserted")); //se doi khi co method phu hop
            }
            if (status == -1)
            {
                return BadRequest(new ResponseDTO(400, "Question number is lower than expected"));
            }
            return BadRequest(new ResponseDTO(400, "Error when create exam paper"));
        }

        [HttpPost("autoGenerate")]
        public async Task<IActionResult> CreateExamAuto(CreateExamAutoInput input)
        {
            if(await _examService.GetExamById(input.ExamId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Exam not existed"));
            }
            int status = await _examService.CreateExamPaperAuto(input);
            if (status == 1)
            {
                return Created(nameof(CreateExamByHand), new ResponseDTO(201, "Successfully inserted")); //se doi khi co method phu hop
            }
            if (status == -1)
            {
                return BadRequest(new ResponseDTO(400, "Question number in bank is lower than expected"));
            }
            return BadRequest(new ResponseDTO(400, "Error when create exam paper"));
        }

        [HttpGet("examInfor/{id:int}")]
        public async Task<ActionResult<ExamResponse>> GetExamById(int id)
        {
            var exam = await _examService.getExamById(id);
            if(exam == null)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            return Ok(_mapper.Map<ExamResponse>(exam));
        }
    }
}