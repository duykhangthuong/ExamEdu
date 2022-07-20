using System;

namespace BackEnd.DTO.ExamDTO
{
    public class ExamDetailResponse
    {
        public string ExamName { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public DateTime ExamDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DurationInMinute { get; set; }
        public string Room { get; set; }
        public bool isFinalExam { get; set; }
        public bool IsCancelled { get; set; }
        public string ProctorFullName { get; set; }
        public string ProctorEmail { get; set; }
        public string SupervisorEmail { get; set; }
    }
}