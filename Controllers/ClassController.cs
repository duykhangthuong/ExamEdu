using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.ClassDTO;
using BackEnd.Services;
using examedu.Services;
using examedu.Services.Classes;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.ClassDTO;
using ExamEdu.DTO.ClassModuleDTO;
using ExamEdu.DTO.PaginationDTO;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace examedu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {

        private readonly IClassService _classService;
        private readonly IClassModuleService _classModuleService;
        private readonly IMapper _mapper;
        private readonly IModuleService _moduleService;
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;

        public ClassController(IClassService classService,
                               IMapper mapper,
                               IModuleService moduleService,
                               IStudentService studentService,
                               ITeacherService teacherService,
                               IClassModuleService classModuleService)
        {
            _classService = classService;
            _mapper = mapper;
            _moduleService = moduleService;
            _teacherService = teacherService;
            _studentService = studentService;
            _classModuleService = classModuleService;
        }


        [HttpGet("{teacherId:int}/{moduleId:int}")]
        public async Task<IActionResult> GetClasses(int teacherId, int moduleId, [FromQuery] PaginationParameter paginationParameter)
        {
            //If teacher does not exist return bad request
            if (await _teacherService.IsTeacherExist(teacherId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Teacher not found"));
            }
            //If module does not exist return bad request
            if (await _moduleService.getModuleByID(moduleId) is null)
            {
                return BadRequest(new ResponseDTO(400, "Module not found"));
            }

            var classes = await _classService.GetClasses(teacherId, moduleId, paginationParameter);

            if (classes.Item1 == 0)
            {
                return NotFound(new ResponseDTO(404, "No class found"));
            }
            //Map classes to ClassNameResponse
            var classesResponse = _mapper.Map<IEnumerable<ClassNameResponse>>(classes.Item2);

            //Return the classes in a pagination response
            return Ok(new PaginationResponse<IEnumerable<ClassNameResponse>>(classes.Item1, classesResponse));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses([FromQuery] PaginationParameter paginationParameter)
        {
            var classes = await _classService.GetAllClasses(paginationParameter);

            if (classes.Item1 == 0)
            {
                return NotFound(new ResponseDTO(404, "No class found"));
            }
            //Map classes to ClassNameResponse
            var classesResponse = _mapper.Map<IEnumerable<ClassNameResponse>>(classes.Item2);

            //Return the classes in a pagination response
            return Ok(new PaginationResponse<IEnumerable<ClassNameResponse>>(classes.Item1, classesResponse));
        }

        [HttpPost("createClass")]
        public async Task<ActionResult> CreateNewClass([FromBody] CreateClassInput input)
        {
            if (await _classService.IsClassNameExist(input.ClassName))
            {
                return BadRequest(new ResponseDTO(400, "Class already exist"));
            }

            foreach (var moduleTeacherStudentId in input.ModuleTeacherStudentIds)
            {
                foreach (int studentId in moduleTeacherStudentId.StudentIds)
                {
                    if (_studentService.CheckStudentExist(studentId) == false)
                    {
                        return NotFound(new ResponseDTO(404, "Student not exist"));
                    }
                }
            }

            foreach (var moduleTeacherId in input.ModuleTeacherStudentIds)
            {
                if (await _teacherService.IsTeacherExist(moduleTeacherId.TeacherId) == false)
                {
                    return NotFound(new ResponseDTO(404, "Teacher not exist"));
                }
                if (_moduleService.IsModuleExist(moduleTeacherId.ModuleId) == false)
                {
                    return NotFound(new ResponseDTO(404, "Module not exist"));
                }
            }

            if (input.StartDay > input.EndDay)
            {
                return BadRequest(new ResponseDTO(400, "Start day must be less than end day"));
            }

            Class classInput = _mapper.Map<Class>(input);
            int rs = await _classService.CreateNewClass(classInput);
            if (rs == 0 || rs == -1)
            {
                return BadRequest(new ResponseDTO(400, "Failed to send request"));
            }

            return Created("", new ResponseDTO(201, "Create class successfully"));
        }

        [HttpGet("{classId:int}")]
        public async Task<IActionResult> GetClass(int classId)
        {
            if (await _classService.IsClassExist(classId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Class not found"));
            }

            //basic infor
            Class basicInfor = await _classService.GetClassBasicInforById(classId);

            //list class module
            List<ClassModule> listClassModule = await _classModuleService.GetClassModuleList(classId);

            basicInfor.ClassModules = listClassModule;

            var ClassModulesList = new List<ClassModuleResponse2>();

            foreach (var classModule in listClassModule)
            {
                ClassModulesList.Add(new ClassModuleResponse2
                {
                    ClassModuleId = classModule.ClassModuleId,
                    ModuleId = classModule.ModuleId,
                    ModuleCode = classModule.Module.ModuleCode,
                    ModuleName = classModule.Module.ModuleName,
                    TeacherName = classModule.Teacher.Fullname
                });
            }

            ClassResponse classResponse = new ClassResponse()
            {
                ClassId = classId,
                ClassName = basicInfor.ClassName,
                CreatedAt = basicInfor.CreatedAt,
                StartDay = basicInfor.StartDay,
                EndDay = basicInfor.EndDay,
                ClassModules = ClassModulesList
            };


            //var result = _mapper.Map<ClassResponse>(basicInfor);

            return Ok(classResponse);

        }
        
        [HttpPut("update/basic_infor")]
        public async Task<ActionResult> UpdateClassBasicInfor([FromBody] ClassBasicInforInput input)
        {
            if (await _classService.IsClassExist(input.ClassId) == false)
            {
                return BadRequest(new ResponseDTO(400, "Class not found"));
            }

            Class classUpdated = _mapper.Map<Class>(input);

            int status = await _classService.UpdateClassBasicInfor(classUpdated);

            if (status == 1)
            {
                return Ok(new ResponseDTO(200, "Class successfully updated"));
            }

            return BadRequest(new ResponseDTO(400, "Error when update class"));
        }
    }
}