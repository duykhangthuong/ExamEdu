using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class AnswerInput
    {
        [Required]
        public string AnswerContent { get; set; }
        public bool isCorrect { get; set; }
    }
}