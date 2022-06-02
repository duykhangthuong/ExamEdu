using examedu.DTO.ExamDTO;

namespace ExamEdu.DTO.ExamDTO
{
    public class UpdateExamInfoResponse : ExamResponse
    {
        public string Room { get; set; }
        public int ProctorId { get; set; }
        public string Password {get;set;}
    }
}