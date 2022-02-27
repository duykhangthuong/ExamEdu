using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExamEdu.DTO.ExamDTO
{
    public class CreateExamInfoInput
    {

        //Exam name required
        [Required]
        public string ExamName { get; set; }
        //Description
        public string Description { get; set; }
        //Duration in minute
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int DurationInMinute { get; set; }
        //Exam date
        [Required]
        public DateTime ExamDay { get; set; }
        //Room
        public string Room { get; set; }
        //password
        public string Password { get; set; }
        //Is final exam with default value being false
        public bool isFinalExam { get; set; } = false;
        //Student ids
        [Required]
        public List<int> StudentIds { get; set; }
        //Module
        [Required]
        public int ModuleId { get; set; }
        //Protor id
        [Required]
        public int ProctorId { get; set; }
        //Supervisor id
        [Required]
        public int SupervisorId { get; set; }

    }
}