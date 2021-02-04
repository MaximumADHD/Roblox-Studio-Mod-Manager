using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class FileManifest : Dictionary<string, string>
    {
        public string RawData { get; set; }

        private FileManifest(string data)
        {
            using (var reader = new StringReader(data))
            {
                bool eof = false;

                var readLine = new Func<string>(() =>
                {
                    string line = reader.ReadLine();

                    if (line == null)
                        eof = true;

                    return line;
                });

                while (!eof)
                {
                    string path = readLine();
                    string signature = readLine();

                    if (eof)
                        break;
                    else if (path.StartsWith("ExtraContent", Program.StringFormat))
                        path = path.Replace("ExtraContent", "content");

                    Add(path, signature);
                }
            }
            
            RawData = data;
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
