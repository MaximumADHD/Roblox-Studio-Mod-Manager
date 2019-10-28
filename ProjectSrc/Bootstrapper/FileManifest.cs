using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class FileManifest : Dictionary<string, string>
    {
        private FileManifest(string data)
        {
            using (StringReader reader = new StringReader(data))
            {
                string path = "";
                string signature = "";

                while (path != null && signature != null)
                {
                    try
                    {
                        path = reader.ReadLine();
                        signature = reader.ReadLine();

                        if (path == null || signature == null)
                            break;

                        Add(path, signature);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        public static async Task<FileManifest> Get(string branch, string versionGuid)
        {
            string fileManifestUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{versionGuid}-rbxManifest.txt";
            string fileManifestData;

            using (WebClient http = new WebClient())
                fileManifestData = await http.DownloadStringTaskAsync(fileManifestUrl);

            return new FileManifest(fileManifestData);
        }
    }
}
