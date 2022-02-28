using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using examedu.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ModuleDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ModuleController : ControllerBase
    {
         private readonly IModuleService _moduleService;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;        
        public ModuleController(IModuleService moduleService,IMapper mapper,IStudentService studentService)
        {
            _moduleService = moduleService;
            _mapper = mapper;
            _studentService = studentService;
        }
        [HttpGet("{studentId:int}")]
        public async Task<IActionResult> ViewModuleStudentHaveExam(int studentId,[FromQuery]PaginationParameter paginationParameter)
        {
            bool isStudentExist = _studentService.CheckStudentExist(studentId);
            if(isStudentExist==false){
                return NotFound(new ResponseDTO(404, "Student not found"));
            }
            (int totalRecord, IEnumerable<Module> modules)  =
                            await _moduleService.GetAllModuleStudentHaveExam(studentId,paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Student doesn't have exam on any module"));
            }
            IEnumerable<ModuleResponse> modulesResponses = _mapper.Map<IEnumerable<ModuleResponse>>(modules);

            return Ok(new PaginationResponse<IEnumerable<ModuleResponse>>(totalRecord, modulesResponses));
        }
        
    }
}