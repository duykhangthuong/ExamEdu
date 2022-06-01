using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.InvigilateDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.Hubs;
using ExamEdu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace examedu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvigilateController : ControllerBase
    {
        private readonly IHubContext<NotifyHub> _notifyHub;
        private readonly IExamService _examService;

        public InvigilateController(IHubContext<NotifyHub> notifyHub, IExamService examService)
        {
            _notifyHub = notifyHub;
            _examService = examService;
        }

        [HttpPost("GenerateRoomId")]
        public async Task<IActionResult> GenerateRoomId(ExamRoomInfor examRoomInfor)
        {
            Exam examInfor = await _examService.getExamById(examRoomInfor.ExamId);
            if (examInfor == null)
            {
                return BadRequest(new ResponseDTO(400, "Exam does not exist"));
            }
            if(examInfor.Room != null && examInfor.Room != "")
            {
                return BadRequest(new ResponseDTO(400, "Exam has been generated room"));
            }
            string roomId = System.Guid.NewGuid().ToString();
            if(await _examService.UpdateExamRoom(examRoomInfor.ExamId, roomId) == 1)
            {
                return Created(nameof(GenerateRoomId), new ResponseDTO(201, "Successfully Generate"));
            }
            return BadRequest(new ResponseDTO(400, "Failed to Generate"));
        }
        [HttpGet("roomId/{examId:int}")]
        public async Task<IActionResult> GetRoomId(int examId)
        {
            Exam examInfor = await _examService.getExamById(examId);
            if (examInfor == null)
            {
                return BadRequest(new ResponseDTO(400, "Exam does not exist"));
            }
            ExamRoomInfor examRoomInfor = new ExamRoomInfor();
            examRoomInfor.ExamId = examId;
            examRoomInfor.RoomId = examInfor.Room;
            return Ok(examRoomInfor);
        }
        
    }
}