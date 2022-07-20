using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ExamEdu.Helper.UploadDownloadFiles
{
    public class ImgHelper : IImgHelper
    {
        private readonly string apiUrl;
        public ImgHelper()
        {
            this.apiUrl = "https://api.imgbb.com/1/upload?key=9d9d2b30cc3b327930a52f2044637699";
        }

        public async Task<string> Upload(IFormFile img)
        {
            Stream stream = img.OpenReadStream();
            string fileName =img.FileName;
            long fileLength = img.Length;
            string fileType = img.ContentType;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(stream)
                    {
                        Headers =
                    {
                        ContentLength = fileLength,
                        ContentType = new MediaTypeHeaderValue(fileType)
                    }
                    }, "image", fileName);

                    var response = await client.PostAsync(this.apiUrl, content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic dec = JsonConvert.DeserializeObject(responseBody);
                    string url = dec.data.url;
                    return url;
                }
            }
        }
    }
}