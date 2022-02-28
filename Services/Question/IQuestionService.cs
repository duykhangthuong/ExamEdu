using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using examedu.DTO.QuestionDTO;

namespace examedu.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionResponse>> GetQuestionByModuleLevel(int moduleID, int levelID, bool isFinalExam);
    }
}