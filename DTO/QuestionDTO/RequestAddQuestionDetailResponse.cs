using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class RequestAddQuestionDetailResponse
    {
        public int AddQuestionRequestId { get; set; }
        public string ModuleName { get; set; }
        public bool IsFinalExamBank { get; set; }
        public ICollection<QuestionInRequestResponse> Questions { get; set; }
    }
}