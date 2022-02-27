using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class AddQuestionRequest
    {
        public int AddQuestionRequestId { get; set; }
        public string  Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int RequesterId { get; set; }
        public Teacher Requester {get;set;}
        public int? ApproverId { get; set; }
        public Teacher Approver { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<FEQuestion> FEQuestions { get; set; }
        
    }
}