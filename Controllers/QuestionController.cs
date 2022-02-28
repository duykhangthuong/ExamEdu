using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.QuestionDTO;
using examedu.Services;
using ExamEdu.DTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public QuestionController(IQuestionService questionService, IModuleService moduleService
        ,ILevelService levelService)
        {
            _moduleService = moduleService;
            _levelService = levelService;
            _questionService = questionService;
            
        }

        [HttpGet("{moduleID:int}/{levelID:int}/{isFinalExam:bool}")]
        public async Task<ActionResult<List<QuestionResponse>>> ViewQuestionBank(int moduleID, int levelID, bool isFinalExam)
        {
            if(await _moduleService.GetModuleByID(moduleID) == null )
            {
                return NotFound(new ResponseDTO(404, "Module Not Found"));
            }
            if (await _levelService.getLevelByID(levelID) == null)
            {
                return NotFound(new ResponseDTO(404, "Level Not Found"));
            }
            
            List<QuestionResponse> listResponse = new List<QuestionResponse>();

            listResponse = await _questionService.GetQuestionByModuleLevel(moduleID,levelID, isFinalExam);
            if(listResponse.Count <= 0)
            {
                return NotFound(new ResponseDTO(404, "This Bank is empty"));
            }
            return Ok(listResponse);

        }
    }
}