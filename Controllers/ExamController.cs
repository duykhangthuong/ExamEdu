using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Services;
using examedu.DTO.ExamDTO;
using examedu.DTO.StudentDTO;
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
        private readonly IModuleService _moduleService;
        private readonly ITeacherService _teacherService;
        private readonly IMapper _mapper;
        public ExamController(IExamService examService,
                                IMapper mapper,
                                IStudentService studentService,
                                IModuleService moduleService,
                                ITeacherService teacherService)
        {
            _teacherService = teacherService;
            _moduleService = moduleService;
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
            if (await _examService.getExamById(input.ExamId) is null)
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
            if (exam == null)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            return Ok(_mapper.Map<ExamResponse>(exam));
        }

        [HttpGet("progressExam/{classModuleId:int}")]
        public async Task<IActionResult> GetProgressExam(int classModuleId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Exam> progressExams) = await _examService.GetExamsByClassModuleId(classModuleId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            IEnumerable<ProgressExamResponse> progressExamResponses = _mapper.Map<IEnumerable<ProgressExamResponse>>(progressExams);

            return Ok(new PaginationResponse<IEnumerable<ProgressExamResponse>>(totalRecord, progressExamResponses));
        }

        /// <summary>
        /// Insert exam's information into the database
        /// </summary>
        /// <param name="input">Exam's information</param>
        /// <returns>
        /// 201: Successfully inserted, exam's id
        /// 400: Error when insert
        /// </returns>
        [HttpPost("createExamInfo")]
        public async Task<IActionResult> CreateExamInfo([FromBody] CreateExamInfoInput input)
        {
            //Check if module exist
            if (await _moduleService.getModuleByID(input.ModuleId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Module does not exist"));
            }
            //Check if student exist
            foreach (var studentId in input.StudentIds)
            {
                //If student does not exist return BadRequest
                if (_studentService.CheckStudentExist(studentId) == false)
                {
                    return BadRequest(new ResponseDTO(400, "Student does not exist"));
                }
            }
            //Check if proctor exist
            if (await _teacherService.IsTeacherExist(input.ProctorId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Proctor does not exist"));
            }
            //Check if supervisor exist
            if (await _teacherService.IsTeacherExist(input.SupervisorId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Supervisor does not exist"));
            }
            //Check if grader exist
            if (await _teacherService.IsTeacherExist(input.GraderId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Grader does not exist"));
            }
            //Now we insert exam
            Exam exam = _mapper.Map<Exam>(input);
            Tuple<int, int> insertResult = await _examService.CreateExamInfo(exam);
            if (insertResult.Item1 < 1)
            {
                return BadRequest(new ResponseDTO(400, "Error when insert exam"));
            }
            return Created(nameof(CreateExamInfo), new CreateExamInfoResponse(201, "Exam information successfully created", insertResult.Item2));
        }
        //get result of an exam
        [HttpGet("result/{examId:int}")]
        public async Task<IActionResult> GetResultExam(int examId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<StudentMarkResponse> studentMarkResponse) = await _examService.GetResultExamByExamId(examId, paginationParameter);

            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Student mark not found"));
            }

            return Ok(new PaginationResponse<IEnumerable<StudentMarkResponse>>(totalRecord, studentMarkResponse));
        }
        [HttpGet("result/report/{examId:int}/{classModuleId:int}")]
        public async Task<ActionResult> ExportExamMarkReport(int examId,int classModuleId)
        {
            var studentListMark= await _examService.GetResultExamListByExamId(examId);
            if(studentListMark.Count()==0){
                return NotFound(new ResponseDTO(404, "There is no student taking this exam "));
            }
            var stream = await _examService.GenerateExamMarkReport(examId, classModuleId);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
        }

        [HttpPut("update-exam-info")]
        public async Task<ActionResult> UpdateExam([FromBody] UpdateExamInfoInput input)
        {
            if (await _examService.getExamById(input.ExamId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Exam not found"));
            }
            Exam exam = _mapper.Map<Exam>(input);
            int status = await _examService.UpdateExam(exam);
            if (status == 1)
            {
                return Ok(new ResponseDTO(200, "Exam successfully updated"));
            }
            return BadRequest(new ResponseDTO(400, "Error when update exam"));
        }
    }
}