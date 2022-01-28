using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.DTO.ExamDTO
{
    public class CreateExamAutoInput
    {
        public bool IsFinal { get; set; }
        public int VariantNumber { get; set; }

        public int ExamId { get; set; }
        public Dictionary<int, int> NumberOfMCQuestionByLevel { get; set; }
        public Dictionary<int, int> NumberOfNonMCQuestionByLevel { get; set; }
        
        public Dictionary <int, decimal> MarkByLevel { get; set; }
    }
}