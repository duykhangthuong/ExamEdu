using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.QuestionDTO;
using BackEnd.Services;
using examedu.DTO.QuestionDTO;
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
    // [Authorize]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IModuleService _moduleService;
        private readonly ILevelService _levelService;
        private readonly ITeacherService _teacherService;
        private readonly IMapper _mapper;

        public QuestionController(IQuestionService questionService,
                                  IModuleService moduleService,
                                  ILevelService levelService,
                                  ITeacherService teacherService,
                                  IMapper mapper)
        {
            _moduleService = moduleService;
            _levelService = levelService;
            _questionService = questionService;
            _teacherService = teacherService;
            _mapper = mapper;
        }

        [HttpGet("{moduleID:int}/{levelID:int}/{isFinalExam:bool}")]
        public async Task<ActionResult<List<QuestionResponse>>> ViewQuestionBank(int moduleID, int levelID, bool isFinalExam)
        {
            if (await _moduleService.getModuleByID(moduleID) == null)
            {
                return NotFound(new ResponseDTO(404, "Module Not Found"));
            }
            if (await _levelService.getLevelByID(levelID) == null)
            {
                return NotFound(new ResponseDTO(404, "Level Not Found"));
            }

            List<QuestionResponse> listResponse = new List<QuestionResponse>();

            listResponse = await _questionService.getQuestionByModuleLevel(moduleID, levelID, isFinalExam);
            if (listResponse.Count <= 0)
            {
                return NotFound(new ResponseDTO(404, "This Bank is empty"));
            }
            return Ok(listResponse);

        }

        [HttpPost("request")]
        public async Task<ActionResult> RequestAddQuestionToBank([FromBody] RequestAddQuestionInput input)
        {
            if (await _teacherService.IsTeacherExist(input.RequesterId) == false)
            {
                return NotFound(new ResponseDTO(404, "Requester is not exist"));
            }

            if (!_moduleService.IsModuleExist(input.Questions.First().ModuleId))
            {
                return NotFound(new ResponseDTO(404, "Module is not exist"));
            }

            IEnumerable<int> moduleIds = await _moduleService.GetAllModuleIdByTeacherId(input.RequesterId);
            foreach (var moduleId in moduleIds)
            {
                if (moduleId != input.Questions.First().ModuleId)
                {
                    return NotFound(new ResponseDTO(404, "Requester not teach this module"));
                }
            }

            if (_levelService.IsLevelExist(input.Questions.First().LevelId) == false)
            {
                return NotFound(new ResponseDTO(404, "Level is not exist"));
            }

            AddQuestionRequest addQuestionRequest = _mapper.Map<AddQuestionRequest>(input);
            int rs = await _questionService.InsertNewRequestAddQuestions(addQuestionRequest);
            if (rs == 0)
            {
                return BadRequest(new ResponseDTO(409, "Failed to send request"));
            }

            return Ok(new ResponseDTO(200, "Request add questions success."));
        }

        [HttpGet("requestList")]
        public async Task<ActionResult<PaginationResponse<IEnumerable<RequestAddQuestionResponse>>>> ViewAllRequestAddQuestionBank([FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<AddQuestionRequest> requestList) = await _questionService.GetAllRequestAddQuestionBank(paginationParameter);

            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Request list cannot be found"));
            }

            var requestResponse = _mapper.Map<IEnumerable<RequestAddQuestionResponse>>(requestList);
            foreach (var request in requestResponse)
            {
                if (_questionService.IsFinalExamBank(request.AddQuestionRequestId))
                {
                    request.IsFinalExamBank = true;
                }
                else
                {
                    request.IsFinalExamBank = false;
                }
            }
            return Ok(new PaginationResponse<IEnumerable<RequestAddQuestionResponse>>(totalRecord, requestResponse));
        }
    }
}