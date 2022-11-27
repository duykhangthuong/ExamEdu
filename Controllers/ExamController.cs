using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ExamDTO;
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

        [HttpGet("progressExam/{classModuleId:int}/{moduleId:int}")]
        public IActionResult GetProgressExam(int classModuleId, int moduleId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Exam> progressExams) = _examService.GetExamsByClassModuleId(classModuleId, moduleId, paginationParameter);
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
            Tuple<int, int> insertResult;
            //Try inserting the exam
            //A bit of unclean code, but I'm not bothered enough.
            try
            {
                insertResult = await _examService.CreateExamInfo(exam);
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseDTO(400, e.Message));
            }

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
        public async Task<ActionResult> ExportExamMarkReport(int examId, int classModuleId)
        {
            var studentListMark = await _examService.GetResultExamListByExamId(examId);
            if (studentListMark.Count() == 0)
            {
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

            //Again, better code can be made, but I'm not bothered enough
            int status;
            try
            {
                status = await _examService.UpdateExam(exam);
            }
            catch (Exception e)
            {
                return Conflict(new ResponseDTO(409, e.Message));
            }

            if (status == 1)
            {
                return Ok(new ResponseDTO(200, "Exam successfully updated"));
            }
            return BadRequest(new ResponseDTO(400, "Error when update exam"));
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAllExam([FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Exam> allExam) = await _examService.GetAllExam(paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            IEnumerable<GetAllExam> allExamResponse = _mapper.Map<IEnumerable<GetAllExam>>(allExam);

            return Ok(new PaginationResponse<IEnumerable<GetAllExam>>(totalRecord, allExamResponse));
        }

        [HttpGet("update-exam-info/{examId:int}")]
        public async Task<ActionResult<UpdateExamInfoResponse>> GetExamInfoToUpdate(int examId)
        {
            var exam = await _examService.GetUpdateExam(examId);
            if (exam == null)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            return Ok(_mapper.Map<UpdateExamInfoResponse>(exam));
        }

        [HttpPut("cancel/{examId:int}")]
        public async Task<ActionResult> CancelExam(int examId)
        {
            bool isCancelled = _examService.IsCancelled(examId);
            bool isExist = _examService.IsExist(examId);
            if (isCancelled || isExist == false)
            {
                return NotFound(new ResponseDTO(404, "This exam is already cancelled or is not found"));
            }

            int status = await _examService.CancelExam(examId);

            if (status == 1)
            {
                return Ok(new ResponseDTO(200, "Exam successfully cancelled"));
            }
            else if (status == -1)
            {
                return NotFound(new ResponseDTO(404, "This exam is already done"));
            }
            return BadRequest(new ResponseDTO(400, "Error when cancel exam"));

        }
        [HttpGet("report/{classModuleId:int}/{moduleId:int}")]
        public async Task<IActionResult> ExportModuleProgressTest(int classModuleId, int moduleId)
        {

            var rp = await _examService.GetAllExamResultByClassModuleId(classModuleId, moduleId);
            if (rp.Count() == 0)
            {
                return NotFound(new ResponseDTO(404, "There is no exam in this module"));
            }
            var stream = await _examService.GenerateModuleProgressExamReport(classModuleId, moduleId);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
        }

        [HttpGet("examDetail/{examId:int}")]
        public async Task<IActionResult> GetExamDetailById(int examId)
        {
            var exam = await _examService.GetExamDetailByExamId(examId);
            if (exam == null)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            return Ok(_mapper.Map<ExamDetailResponse>(exam));
        }

        [HttpGet("examProctor/{proctorId:int}")]
        public async Task<IActionResult> GetExamByProctorId(int proctorId, [FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Exam> examList) = await _examService.GetExamByProctorId(proctorId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Exam not found"));
            }
            IEnumerable<ExamProctorResponse> examListResponse = _mapper.Map<IEnumerable<ExamProctorResponse>>(examList);

            return Ok(new PaginationResponse<IEnumerable<ExamProctorResponse>>(totalRecord, examListResponse));
        }
        [HttpGet("SEB")]
        public IActionResult CheckSEB()
        {
            // get header from http request
            var headers = Request.Headers;
            return Ok(headers);
        }
        [HttpPut("maxFinishTime")]
        public async Task<IActionResult> UpdateMaxFinishTime(int examId, int studentId)
        {
            int result = await _examService.UpdateMaxTimeToFinishExamOfStudent(examId, studentId);
            if (result == 0)
            {
                return BadRequest(new ResponseDTO(400, "Not found"));
            }
            else if(result == -1)
            {
                return BadRequest(new ResponseDTO(400, "You have already joined this exam before"));
            }
            return Ok(new ResponseDTO(200, "Update successfully"));
            
        }
    }
}