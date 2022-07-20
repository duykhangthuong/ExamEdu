using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DB.Models
{
    public class CheatingType
    {
        public int CheatingTypeId { get; set; }
        public string CheatingTypeName { get; set; }
        public ICollection<StudentCheating> StudentCheatings { get; set; }
    }
}