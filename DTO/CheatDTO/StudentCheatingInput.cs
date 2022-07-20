using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.DTO.CheatDTO
{
    public class StudentCheatingInput
    {
        public int ExamId { get; set; }

        public string StudentEmail { get; set; }
        public DateTime Time { get; set; }
        public string Comment { get; set; }
        public bool IsComfirmed { get; set; } = false;
        public int CheatingTypeId { get; set; }
    }
}