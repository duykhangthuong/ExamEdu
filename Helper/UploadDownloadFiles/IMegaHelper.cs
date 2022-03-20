using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExamEdu.Helper.UploadDownloadFiles
{
    public interface IMegaHelper
    {
        Task<String> Upload(Stream data, string fileName, string folderName);
        Task<Stream> Download(Uri uri);
    }
}