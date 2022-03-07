using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.QuestionDTO;

namespace BackEnd.DTO.QuestionDTO
{
    public class QuestionInRequestResponse
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; }
        public string QuestionImageURL { get; set; }
        public string LevelName { get; set; }
        public List<AnswerResponse> Answers { get; set; }
    }
}