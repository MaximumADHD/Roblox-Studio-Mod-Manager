using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA2237 // Mark ISerializable types with serializable

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
                    path = reader.ReadLine();
                    signature = reader.ReadLine();

                    if (path == null || signature == null)
                        break;

                    if (path.StartsWith("ExtraContent", Program.StringFormat))
                        path = path.Replace("ExtraContent", "content");

                    Add(path, signature);
                }
            }
        }

        public static async Task<FileManifest> Get(string branch, string versionGuid)
        {
            string fileManifestUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{versionGuid}-rbxManifest.txt";
            string fileManifestData;

            using (WebClient http = new WebClient())
            {
                var download = http.DownloadStringTaskAsync(fileManifestUrl);
                fileManifestData = await download.ConfigureAwait(false);
            }
            
            return new FileManifest(fileManifestData);
        }
    }
}
