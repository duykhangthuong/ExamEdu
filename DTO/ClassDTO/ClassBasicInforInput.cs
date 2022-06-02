using System;

namespace ExamEdu.DTO.ClassDTO
{
    public class ClassBasicInforInput
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime? StartDay { get; set; }
        public DateTime? EndDay { get; set; }
    }
}
