using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamEdu.DB.Models;
using ExamEdu.DTO.StudentAnswerDTO;

namespace ExamEdu.Services
{
    public interface IStudentAnswerService
    {
        Task<int> InsertStudentAnswers(List<StudentAnswer> answers);
        Task<int> InsertFEStudentAnswers(List<StudentFEAnswer> answers);
        Task<List<StudentTextAnswerResponse>> GetStudentTextAnswer(int studentId, int examId);
        Task<List<StudentTextAnswerResponse>> GetStudentFETextAnswer(int studentId, int examId);
    }
}