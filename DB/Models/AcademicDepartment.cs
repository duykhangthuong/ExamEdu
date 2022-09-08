using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class AcademicDepartment
    {
        public int AcademicDepartmentId { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DeactivatedAt { get; set; }

        // Many aca - one role
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<Exam> Exams { get; set; }

    }
}