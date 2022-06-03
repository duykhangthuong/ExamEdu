using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.TeacherDTO;
using BackEnd.Services;
using ExamEdu.DB.Models;
using ExamEdu.DTO;
using ExamEdu.DTO.AcademicDepartmentDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamEdu.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AcademicDepartmentController : ControllerBase
    {
        private readonly IAcademicDepartmentService _academicDepartmentService;
        private readonly IMapper _mapper;

        public AcademicDepartmentController(IAcademicDepartmentService academicDepartmentService, IMapper mapper)
        {
            _academicDepartmentService = academicDepartmentService;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a list of academic departments employee
        /// </summary>
        /// <param name="paginationParameter"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetAcademicDepartmentList()
        {
            
            IEnumerable<AcademicDepartment> listAcademicDepartment = await _academicDepartmentService.GetAcademicDeparment();

            if(listAcademicDepartment.Count() == 0)
            {
                return NotFound(new ResponseDTO(404, "No academic department found"));
            }

            
            IEnumerable<AcademicDepartmentResponse> listAcademicDeparmentResponse = _mapper.Map<IEnumerable<AcademicDepartmentResponse>>(listAcademicDepartment);

            return Ok(listAcademicDeparmentResponse);
        }
    }
}