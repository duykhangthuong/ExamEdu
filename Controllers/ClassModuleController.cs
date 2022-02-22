using System.Threading.Tasks;
using AutoMapper;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ClassModuleDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace examedu.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ClassModuleController : ControllerBase
    {
        private readonly IClassModuleService _classModuleService;
        private readonly IMapper _mapper;

        public ClassModuleController(IClassModuleService classModuleService, IMapper mapper)
        {
            _mapper = mapper;
            _classModuleService = classModuleService;
        }

        //Get ClassModule from ClassModuleId
        [HttpGet("{classModuleId:int}")]
        public async Task<IActionResult> GetClassModuleInfo(int classModuleId)
        {
            ClassModule classModule = await _classModuleService.GetClassModuleInfo(classModuleId);
            if (classModule == null)
            {
                return NotFound(new ResponseDTO(404, "No class module found"));
            }

            ClassModuleResponse classModuleResponse = _mapper.Map<ClassModuleResponse>(classModule);
            return Ok(classModuleResponse);
        }
    }
}