using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.StudentAnswerDTO
{
    public class StudentTextAnswerResponse
    {
        public int StudentId { get; set; }
        public string QuestionContent { get; set; }
        public string QuestionImageURL { get; set; }
        public string StudentAnswer { get; set; }
        public float QuestionMark { get; set; }
    }
}