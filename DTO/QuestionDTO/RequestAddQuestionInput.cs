using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class RequestAddQuestionInput
    {
        [Required]
        public int RequesterId { get; set; }
        public string Description { get; set; }
        [Required]
        public bool isFinalExam { get; set; }
        [Required]
        public ICollection<QuestionInput> Questions { get; set; }
    }
}