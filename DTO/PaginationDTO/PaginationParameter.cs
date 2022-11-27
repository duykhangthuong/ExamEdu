using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.DTO.PaginationDTO
{
    public class PaginationParameter
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1")]
        public int PageNumber { get; set; } = 1;
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than or equal to 1")]
        public int PageSize { get; set; } = 10;
        private string _searchName = "";
        public string SearchName
        {
            get { return _searchName; }
            set
            {
                if (value != null)
                {
                    value.Trim();
                    _searchName = value;
                }
                else _searchName = "";
            }
        }
    }
}