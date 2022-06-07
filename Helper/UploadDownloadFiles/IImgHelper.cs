using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.Helper.UploadDownloadFiles
{
    public interface IImgHelper
    {
        Task<string> Upload(Stream stream, string fileName, long fileLength, string fileType);
    }
}