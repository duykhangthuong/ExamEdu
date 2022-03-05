using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Services;
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
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;
        public ModuleController(IModuleService moduleService, IMapper mapper, IStudentService studentService, ITeacherService teacherService)
        {
            _moduleService = moduleService;
            _mapper = mapper;
            _studentService = studentService;
            _teacherService = teacherService;
        }

        [HttpGet("{studentId:int}")]
        public async Task<IActionResult> ViewModuleStudentHaveExam(int studentId, [FromQuery] PaginationParameter paginationParameter)
        {
            bool isStudentExist = _studentService.CheckStudentExist(studentId);
            if (isStudentExist == false)
            {
                return NotFound(new ResponseDTO(404, "Student not found"));
            }
            (int totalRecord, IEnumerable<ModuleResponse> modules) =
                            await _moduleService.getAllModuleStudentHaveLearn(studentId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "Student doesn't study any module"));
            }
            // IEnumerable<ModuleResponse> modulesResponses = _mapper.Map<IEnumerable<ModuleResponse>>(modules);

            return Ok(new PaginationResponse<IEnumerable<ModuleResponse>>(totalRecord, modules));
        }


        /// <summary>
        /// Get all module in the db with pagination config
        /// </summary>
        /// <param name="paginationParameter">Pagination parameters</param>
        /// <returns>200: List of Module with pagination / 404: No Module found</returns>
        [HttpGet]
        public async Task<IActionResult> ViewAllModule([FromQuery] PaginationParameter paginationParameter)
        {
            (int totalRecord, IEnumerable<Module> modules) = await _moduleService.getModules(paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No module found"));
            }
            IEnumerable<ModuleInformationResponse> moduleInformationResponses = _mapper.Map<IEnumerable<ModuleInformationResponse>>(modules);
            return Ok(new PaginationResponse<IEnumerable<ModuleInformationResponse>>(totalRecord, moduleInformationResponses));
        }

        [HttpGet("teacher/{teacherId:int}")]
        public async Task<IActionResult> GetModules(int teacherId, [FromQuery] PaginationParameter paginationParameter)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher does not exist"));
            }

            (int totalRecord, IEnumerable<Module> modules) = await _moduleService.getModulesByTeacherId(teacherId, paginationParameter);
            if (totalRecord == 0)
            {
                return NotFound(new ResponseDTO(404, "No module found"));
            }
            //Map to moduleResponse
            IEnumerable<ModuleResponse> moduleResponses = _mapper.Map<IEnumerable<ModuleResponse>>(modules);
            return Ok(new PaginationResponse<IEnumerable<ModuleResponse>>(totalRecord, moduleResponses));

        }

        /// <summary>
        /// Create a new module from module Input
        /// </summary>
        /// <param name="moduleInput">Detail of module composing of: moduleCode, moduleName</param>
        /// <returns>201: Module Created / 400: Something went wrong / 409: Module already exists</returns>
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] ModuleInput moduleInput)
        {
            //Check if module name is exist
            bool isModuleExist = await _moduleService.getModuleByCode(moduleInput.ModuleCode) != null;

            if (isModuleExist)
            {
                return Conflict(new ResponseDTO(409, "Module already exists"));
            }

            int result = await _moduleService.InsertNewModule(moduleInput);

            if (result != 1)
            {
                return BadRequest(new ResponseDTO(400, "Something went wrong"));
            }

            return CreatedAtAction(nameof(ViewAllModule), new ResponseDTO(201, "Module created"));

        }

        /// <summary>
        /// Update a module with information from moduleInput
        /// </summary>
        /// <param name="moduleInput">Detail of module composing of: moduleCode, moduleName</param>
        /// <returns>200: Module updated / 400: Something went wrong / 404: Module not found</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateModule([FromBody] ModuleInput moduleInput)
        {
            //Check if module exist
            bool isModuleExist = await _moduleService.getModuleByCode(moduleInput.ModuleCode) != null;

            if (!isModuleExist)
            {
                return NotFound(new ResponseDTO(404, "Module not found"));
            }

            int result = await _moduleService.UpdateModule(moduleInput);
            if (result != 1)
            {
                return BadRequest(new ResponseDTO(400, "Something went wrong"));
            }

            return Ok(new ResponseDTO(200, "Module updated"));
        }

        /// <summary>
        /// Delete a module by id
        /// </summary>
        /// <param name="id">The id of the module</param>
        /// <returns>200: Module deleted / 400: Something went wrong / 404: Module not found</returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            //Check if module exist
            bool isModuleExist = await _moduleService.getModuleByID(id) != null;

            if (!isModuleExist)
            {
                return NotFound(new ResponseDTO(404, "Module not found"));
            }

            int result = await _moduleService.DeleteModule(id);

            if (result != 1)
            {
                return BadRequest(new ResponseDTO(400, "Something went wrong"));
            }

            return Ok(new ResponseDTO(200, "Module deleted"));
        }
    }
}