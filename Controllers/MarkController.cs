using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.Services;
using ExamEdu.DTO;
using ExamEdu.DTO.MarkDTO;
using Microsoft.AspNetCore.Mvc;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarkController : ControllerBase
    {
        private readonly IMarkService _markService;
        public MarkController(IMarkService markService)
        {
            _markService = markService;
        }

        [HttpPut("textAnswer")]
        public async Task<IActionResult> UpdateMarkByTextAnswer(int studentId, int examId, List<TextAnswerMarkInput> markInputs)
        {
            int result = await _markService.UpdateStudentMarkByTextAnswer(studentId, examId, markInputs);
            if (result == -1)
            {
                return BadRequest(new ResponseDTO(400, "Your mark input is invalid"));
            }
            else if (result >= 1)
            {
                return Ok(new ResponseDTO(200, "Update student mark successfully"));
            }
            else
            {
                return BadRequest(new ResponseDTO(400, "Some error happen"));
            }
        }

    }
}