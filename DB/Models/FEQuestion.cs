using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class FEQuestion
    {
        public int FEQuestionId { get; set; }
        public string QuestionContent { get; set; }
        public string QuestionImageURL { get; set; }
        public bool? isApproved { get; set; } = null;
        public DateTime? ApproveAt { get; set; }

        // Many question - one level
        public int LevelId { get; set; }
        public Level Level { get; set; }

        // Many question - one module
        public int ModuleId { get; set; }
        public Module Module { get; set; }

        // Many question - one questionType
        public int QuestionTypeId { get; set; }
        public QuestionType QuestionType { get; set; }

        // Many question - one AddQuestionRequest
        public int AddQuestionRequestId { get; set; }
        public AddQuestionRequest AddQuestionRequest { get; set; }

        // 1 Question - many answer
        public ICollection<FEAnswer> FEAnswers { get; set; }

        // Many-Many Exam
        public ICollection<Exam> Exams { get; set; }
        public ICollection<Exam_FEQuestion> Exam_FEQuestions { get; set; }
    }
}