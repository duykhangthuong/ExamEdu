using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DTO.TeacherDTO
{
    public class AssignTeacherInput
    {
        public int HeadOfDepartmentId { get; set; }
        public int AddQuestionRequestId { get; set; }
        public int ApproverId { get; set; }
    }
}