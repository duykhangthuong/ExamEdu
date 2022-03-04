using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class QuestionInput
    {
        [Required]
        public string QuestionContent { get; set; }
        public string QuestionImageURL { get; set; }
        [Required]
        public int QuestionTypeId { get; set; }
        [Required]
        public int LevelId { get; set; }
        [Required]
        public int ModuleId { get; set; }
        [Required]
        public List<AnswerInput> Answers { get; set; }
    }
}