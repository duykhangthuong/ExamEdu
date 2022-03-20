using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.ExamQuestionsDTO
{
    public class ExamQuestionsResponse
    {
        public int ExamId { get; set; }
        public bool isFinalExam { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; }
        public int DurationInMinute { get; set; }
        //  public int ExamCode { get; set; } //xoa sau
        public List<QuestionAnswerResponse> QuestionAnswer { get; set; }

       
    }
}