using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.Services
{
    public interface IMarkService
    {
        Task<Tuple<int,decimal>> getMCQMarkNonFinal(int examId, int studentId);
        Task<Tuple<int,decimal>> getMCQMarkFinal(int examId, int studentId);
        Task<int> SaveStudentMark(decimal mark, int examId, int studentId);
    }
}