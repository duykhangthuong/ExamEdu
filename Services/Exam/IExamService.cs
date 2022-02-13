using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.ExamDTO;
using ExamEdu.DB.Models;
using ExamEdu.DTO.PaginationDTO;

namespace ExamEdu.Services
{
    public interface IExamService
    {
        Task<Tuple<int, IEnumerable<Exam>>> getExamByStudentId(int studentId, PaginationParameter paginationParameter);
        Task<int> CreateExamPaperByHand(CreateExamByHandInput input);
        Task<int> CreateExamPaperAuto(CreateExamAutoInput input);
        Task<Exam> getExamById(int id);
        bool IsFinalExam(int examId);
        Task<Tuple<int, IEnumerable<Exam>>> GetExamsByClassModuleId(int classModuleId, PaginationParameter paginationParameter);
    }
}