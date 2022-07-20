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
using ExamEdu.DTO.QuestionDTO;
using ExamEdu.Helper.UploadDownloadFiles;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace examedu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IModuleService _moduleService;
        private readonly ILevelService _levelService;
        private readonly ITeacherService _teacherService;
        private readonly IMapper _mapper;
        private readonly IImgHelper _imgHelper;
        public QuestionController(IQuestionService questionService,
                                  IModuleService moduleService,
                                  ILevelService levelService,
                                  ITeacherService teacherService,
                                  IMapper mapper,
                                  IImgHelper imgHelper)
        {
            _moduleService = moduleService;
            _levelService = levelService;
            _questionService = questionService;
            _teacherService = teacherService;
            _mapper = mapper;
            _imgHelper = imgHelper;
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
            if (moduleIds.AsQueryable().Any(id => id == input.Questions.First().ModuleId) == false)
            {
                return NotFound(new ResponseDTO(404, "Requester not teach this module"));
            }

            foreach (var levelId in input.Questions.Select(q => q.LevelId).ToList())
            {
                if (_levelService.IsLevelExist(levelId) == false)
                {
                    return NotFound(new ResponseDTO(404, "Level is not exist"));
                }
            }

            AddQuestionRequest addQuestionRequest = _mapper.Map<AddQuestionRequest>(input);
            int rs = await _questionService.InsertNewRequestAddQuestions(addQuestionRequest);
            if (rs == 0)
            {
                return BadRequest(new ResponseDTO(400, "Failed to send request"));
            }

            return Ok(new ResponseDTO(200, "Request add questions success."));
        }

        [HttpGet("requestList")]
        public async Task<ActionResult<PaginationResponse<IEnumerable<RequestAddQuestionResponse>>>> ViewAllRequestAddQuestionBank([FromQuery] int id, [FromQuery] PaginationParameter paginationParameter)
        {
            if (await _teacherService.IsHeadOfDepartment(id) == false)
            {
                return StatusCode(403, new ResponseDTO(403));
            }

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
                    if (await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, true) == null)
                    {
                        continue;
                    }
                    request.ModuleName = await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, true);
                }
                else
                {
                    request.IsFinalExamBank = false;
                    if (await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, false) == null)
                    {
                        continue;
                    }
                    request.ModuleName = await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, false);
                }
            }
            return Ok(new PaginationResponse<IEnumerable<RequestAddQuestionResponse>>(totalRecord, requestResponse));
        }

        [HttpPut("assignTeacher")]
        public async Task<IActionResult> AssignTeacherToApproveRequest([FromQuery] int id, int addQuestionRequestId, int teacherId)
        {
            if (await _teacherService.IsHeadOfDepartment(id) == false)
            {
                return StatusCode(403, new ResponseDTO(403));
            }

            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return NotFound(new ResponseDTO(404, "Teacher is not exist"));
            }

            int rs = await _questionService.AssignTeacherToApproveRequest(addQuestionRequestId, teacherId);
            if (rs == -1)
            {
                return Conflict(new ResponseDTO(409, "Request has already assigned"));
            }
            if (rs == -2)
            {
                return NotFound(new ResponseDTO(404, "Request is not exist"));
            }
            else if (rs == 0)
            {
                return BadRequest(new ResponseDTO(400, "Failed to send request"));
            }
            else
            {
                return Ok(new ResponseDTO(200, "Assign teacher success"));
            }
        }

        [HttpGet("request/{addQuestionRequestId:int}")]
        public async Task<ActionResult<RequestAddQuestionDetailResponse>> ViewRequestAddQuestionDetail(int addQuestionRequestId)
        {
            if (await _questionService.IsRequestExist(addQuestionRequestId) == false)
            {
                return NotFound(new ResponseDTO(404, "Request is not exist"));
            }

            AddQuestionRequest request = await _questionService.GetRequestAddQuestionBankDetail(addQuestionRequestId);
            var requestResponse = _mapper.Map<RequestAddQuestionDetailResponse>(request);

            if (_questionService.IsFinalExamBank(addQuestionRequestId))
            {
                requestResponse.IsFinalExamBank = true;
            }
            else
            {
                requestResponse.IsFinalExamBank = false;
            }

            return Ok(requestResponse);
        }

        [HttpPut("approveRequest")]
        public async Task<IActionResult> ApproveRequestAddQuestion([FromBody] IEnumerable<QuestionToApproveInput> inputList)
        {
            int rs = 0;
            foreach (var input in inputList)
            {
                rs = await _questionService.ApproveQuestion(input);
                if (rs == -1)
                {
                    return NotFound(new ResponseDTO(404, "Question is not exist or has been approved"));
                }
                else if (rs == 0)
                {
                    return BadRequest(new ResponseDTO(400, "Failed to send request"));
                }
            }
            return Ok(new ResponseDTO(200, "Approve request success"));
        }

        [HttpGet("requestList/{approverId:int}")]
        public async Task<ActionResult<PaginationResponse<IEnumerable<RequestAddQuestionListByApproverResponse>>>> ViewAllRequestAddQuestionBankByApproverId(int approverId, [FromQuery] PaginationParameter paginationParameter)
        {
            if (await _teacherService.IsTeacherExist(approverId) == false)
            {
                return NotFound(new ResponseDTO(404, "Approver is not exist"));
            }

            (int totalRecord, IEnumerable<AddQuestionRequest> requestList) = await _questionService.GetAllRequestAddQuestionByApproverId(approverId, paginationParameter);

            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Request list cannot be found"));
            }

            var requestResponse = _mapper.Map<IEnumerable<RequestAddQuestionListByApproverResponse>>(requestList);
            foreach (var request in requestResponse)
            {
                if (_questionService.IsFinalExamBank(request.AddQuestionRequestId))
                {
                    request.IsFinalExamBank = true;
                    if (await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, true) == null)
                    {
                        continue;
                    }
                    request.ModuleName = await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, true);
                }
                else
                {
                    request.IsFinalExamBank = false;
                    if (await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, false) == null)
                    {
                        continue;
                    }
                    request.ModuleName = await _questionService.GetModuleNameByAddQuestionRequestId(request.AddQuestionRequestId, false);
                }
            }
            return Ok(new PaginationResponse<IEnumerable<RequestAddQuestionListByApproverResponse>>(totalRecord, requestResponse));
        }

        [HttpPost("images")]
        public async Task<ActionResult> UploadImage([FromForm] IFormFile[] inputs)
        {
            // var imageInputsList = JsonConvert.DeserializeObject<List<QuestionImageInput>>(inputs);
            if (inputs == null)
            {
                return BadRequest(new ResponseDTO(400, "ImageList is null"));
            }
            // var listImageUrl = new List<QuestionImageResponse>();
            var listImageUrl = new List<string>();

            foreach (var input in inputs)
            {
                if (input == null)
                {
                    return BadRequest(new ResponseDTO(400, "Image is null"));
                }
                if (input.Length == 0)
                {
                    return BadRequest(new ResponseDTO(400, "Image is empty"));
                }
                if (input.FileName.Split('.').Last() != "png" && input.FileName.Split('.').Last() != "jpg" && input.FileName.Split('.').Last() != "jpeg")
                {
                    return BadRequest(new ResponseDTO(400, "Image is not valid"));
                }
                string url = await _imgHelper.Upload(input);
                listImageUrl.Add(url);
            }
            return Ok(listImageUrl);
        }
    }
}