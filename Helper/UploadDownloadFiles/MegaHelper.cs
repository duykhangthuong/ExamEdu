using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace ExamEdu.Helper.UploadDownloadFiles
{
    public class MegaHelper : IMegaHelper
    {
        private readonly MegaApiClient client;
        public MegaHelper(string username, string password)
        {
            client = new MegaApiClient();
            // client.Login(username, password);
        }
        public async Task<String> Upload(Stream data, string fileName, string folderName)
        {
            IEnumerable<INode> nodes = await client.GetNodesAsync();
            INode root = nodes.Single(x => x.Type == NodeType.Root);

            // Search a folder with folderName under root
            var folder = client.GetNodes(root).FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == folderName);

            // Create the "folderName" if not found
            if (folder == null)
            {
                folder = client.CreateFolder(folderName, root);
            }
            
            INode myFile = await client.UploadAsync(data, fileName, folder);
            Uri downloadLink = await client.GetDownloadLinkAsync(myFile);
            return downloadLink.ToString();
        }
        public Task<Stream> Download(Uri uri)
        {
            return client.DownloadAsync(uri);
        }
    }
}