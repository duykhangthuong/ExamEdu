using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.MarkDTO
{
    public class TextAnswerMarkInput
    {
        public int ExamQuestionId { get; set; }
        public float Mark { get; set; }
    }
}