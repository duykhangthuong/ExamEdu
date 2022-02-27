namespace ExamEdu.DTO.ExamDTO
{
    public class CreateExamInfoResponse : ResponseDTO
    {
        public int ExamId { get; set; }

        public CreateExamInfoResponse(int status, string message, int examId) : base(status, message)
        {
            ExamId = examId;
        }
    }
}