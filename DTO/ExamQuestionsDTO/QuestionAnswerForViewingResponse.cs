using System.Collections.Generic;

namespace BackEnd.DTO.ExamQuestionsDTO
{
    public class QuestionAnswerForViewingResponse
    {
        public string QuestionContent { get; set; }
        public string QuestionImageURL { get; set; }
        public string LevelName { get; set; }
        public List<AnswerContentForViewingResponse> Answers { get; set; }
    }
}