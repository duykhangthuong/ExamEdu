using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public bool isHeadOfDepartment { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DeactivatedAt { get; set; }

        // Many teacher - 1 role
        public int RoleId { get; set; }
        public Role Role { get; set; }
        //1 teacher - many addQuestionrequest
        public ICollection<AddQuestionRequest> AddQuestionRequests { get; set; }
        public ICollection<AddQuestionRequest> AddQuestionApproves { get; set; }
        //1 teacher - many Class module
        public ICollection<ClassModule> ClassModules { get; set; }
        
        //1 teacher - many exam
        public ICollection<Exam> ExamsToProc { get; set; }
        public ICollection<Exam> ExamsToGrade { get; set; }

        
    }
}