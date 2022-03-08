using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.QuestionDTO
{
    public class RequestAddQuestionListByApproverResponse
    {
        public int AddQuestionRequestId { get; set; }
        public string Fullname { get; set; } //Requester's name
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string ModuleName { get; set; }
        public bool IsApproved { get; set; }
        public bool IsFinalExamBank { get; set; }
        public int NumberOfQuestion { get; set; }
    }
}