using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class QuestionToApproveInput
    {
        public int QuestionId { get; set; }
        public bool? isApproved { get; set; }
        public string Comment { get; set; }
        public bool IsFinalExamBank { get; set; }
    }
}