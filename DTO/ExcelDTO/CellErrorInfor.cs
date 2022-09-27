using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace examedu.DTO.ExcelDTO
{
    public class CellErrorInfor
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string ErrorDetail { get; set; }
    }
}