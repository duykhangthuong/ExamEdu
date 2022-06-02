using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BackEnd.DTO.ClassModuleDTO;

namespace BackEnd.DTO.ClassDTO
{
    public class CreateClassInput
    {
        [Required]
        public string ClassName { get; set; }
        [Required]
        public DateTime StartDay { get; set; }
        [Required]
        public DateTime EndDay { get; set; }
        [Required]
        public ICollection<ModuleTeacherStudentInput> ModuleTeacherStudentIds { get; set; }
    }
}