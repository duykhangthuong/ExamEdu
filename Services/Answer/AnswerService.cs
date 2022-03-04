using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ExamEdu.DB;

namespace BackEnd.Services.Answer
{
    public class AnswerService : IAnswerService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public AnswerService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        
    }
}