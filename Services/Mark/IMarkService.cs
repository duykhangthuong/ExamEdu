using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.Services
{
    public interface IMarkService
    {
        Task<Tuple<int,decimal>> GetMCQMarkNonFinal(int examId, int studentId);
        Task<Tuple<int,decimal>> GetMCQMarkFinal(int examId, int studentId);
    }
}