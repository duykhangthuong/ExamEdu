using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ExamEdu.Helper.UploadDownloadFiles
{
    public interface IImgHelper
    {
        Task<string> Upload(IFormFile img);
    }
}