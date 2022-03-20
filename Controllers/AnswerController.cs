using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.StudentAnswerDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnswerController : ControllerBase
    {
        private readonly IStudentAnswerService _studentAnswerService;
        private readonly IMapper _mapper;
        private readonly IMarkService _markService;
        private readonly IExamService _examService;
        public AnswerController(IStudentAnswerService studentAnswerService,
                                IMapper mapper,
                                IMarkService markService,
                                IExamService examService)
        {
            _studentAnswerService = studentAnswerService;
            _mapper = mapper;
            _markService = markService;
            _examService = examService;
        }
        [HttpPost("PT")]
        public async Task<IActionResult> SubmitAnswer(List<StudentAnswerInput> answerInputs, int examId, int studentId)
        {
            var answers = _mapper.Map<List<StudentAnswer>>(answerInputs);
            var rs = await _studentAnswerService.InsertStudentAnswers(answers);
            if (rs == -1)
            {
                return BadRequest(new ResponseDTO(400, "Student is not found"));
            }
            else if (rs == -2)
            {
                return BadRequest(new ResponseDTO(400, "Exam question is not found"));
            }
            (int status, decimal mark) = await _markService.getMCQMarkNonFinal(examId, studentId);

            int res = await _markService.SaveStudentMark(mark, examId, studentId);
            if (res == 1)
                return Ok(new ResponseDTO(200, $"Your multiple choice mark is <b>{mark}</b>. </br>If your exam has essay question, we will inform you later"));
            else return BadRequest(new ResponseDTO(400, "Some error happen"));

        }
        [HttpPost("FE")]
        public async Task<IActionResult> SubmitFEAnswer(List<StudentFEAnswerInput> answerInputs, int examId, int studentId)
        {
            var answers = _mapper.Map<List<StudentFEAnswer>>(answerInputs);
            var rs = await _studentAnswerService.InsertFEStudentAnswers(answers);
            if (rs == -1)
            {
                return BadRequest(new ResponseDTO(400, "Student is not found"));
            }
            else if (rs == -2)
            {
                return BadRequest(new ResponseDTO(400, "Exam FE question is not found"));
            }
            (int status, decimal mark) = await _markService.getMCQMarkFinal(examId, studentId);

            int res = await _markService.SaveStudentMark(mark, examId, studentId);
            if (res == 1)
                return Ok(new ResponseDTO(200, "You have completed the final exam. We will inform your mark later"));
            else return BadRequest(new ResponseDTO(400, "Some error happen"));

        }
        [HttpGet("TextAnswer")]
        public async Task<IActionResult> ViewStudentTextAnswer(int studentId, int examId)
        {
            bool isFinalExam = _examService.IsFinalExam(examId);
            List<StudentTextAnswerResponse> studentTextAnswer = null;
            if (isFinalExam is true)
            {
                studentTextAnswer = await _studentAnswerService.GetStudentFETextAnswer(studentId, examId);
            }
            else
            {
                studentTextAnswer = await _studentAnswerService.GetStudentTextAnswer(studentId, examId);
            }
            if (studentTextAnswer == null)
            {
                return BadRequest(new ResponseDTO(400, "Student don't have text answer"));
            }
            return Ok(studentTextAnswer);
        }

    }
}