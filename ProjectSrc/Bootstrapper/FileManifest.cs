using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class FileManifest
    {
        public Dictionary<string, List<string>> SignatureToFiles;
        public Dictionary<string, string> FileToSignature;

        public static async Task<FileManifest> Get(string branch, string versionGuid)
        {
            string fileManifestUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{versionGuid}-rbxManifest.txt";
            string fileManifestData;

            using (WebClient http = new WebClient())
                fileManifestData = await http.DownloadStringTaskAsync(fileManifestUrl);

            FileManifest result = new FileManifest()
            {
                FileToSignature = new Dictionary<string, string>(),
                SignatureToFiles = new Dictionary<string, List<string>>()
            };

            using (StringReader reader = new StringReader(fileManifestData))
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

                        if (!result.SignatureToFiles.ContainsKey(signature))
                            result.SignatureToFiles.Add(signature, new List<string>());

                        result.SignatureToFiles[signature].Add(path);
                        result.FileToSignature.Add(path, signature);
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
