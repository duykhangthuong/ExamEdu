using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ExamEdu.DTO.QuestionDTO
{
    public class QuestionImageInput
    {
        public int index { get; set; }
        public IFormFile image { get; set; }
    }
}