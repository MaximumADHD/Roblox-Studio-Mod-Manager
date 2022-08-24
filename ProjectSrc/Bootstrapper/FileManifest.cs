using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using RobloxDeployHistory;

namespace RobloxStudioModManager
{
    [Serializable]
    internal class FileManifest : Dictionary<string, string>
    {
        public string RawData { get; set; }

        protected FileManifest(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        
        private FileManifest(string data, bool remapExtraContent = false)
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
                    else if (remapExtraContent && path.StartsWith("ExtraContent", Program.StringFormat))
                        path = path.Replace("ExtraContent", "content");

                    // ~~ AWFUL TEMPORARY HACK. ~~
                    //
                    // This is necessary because SourceSansPro gets incorrectly listed in the root directory,
                    // but also MUST be extracted to both 'StudioFonts/' and 'content/fonts/', so I need to
                    // make sure it unambiguously extracts to the correct locations.

                    if (path.EndsWith(".ttf") && !path.Contains("\\"))
                        path = "StudioFonts\\" + path;

                    Add(path, signature);
                }
            }
            
            RawData = data;
        }

        public static async Task<FileManifest> Get(ClientVersionInfo info, bool remapExtraContent = false)
        {
            Channel channel = info.Channel;
            string versionGuid = info.VersionGuid;

            string fileManifestUrl = $"https://setup.rbxcdn.com/channel/{channel}/{versionGuid}-rbxManifest.txt";
            string fileManifestData;

            using (WebClient http = new WebClient())
            {
                var download = http.DownloadStringTaskAsync(fileManifestUrl);
                fileManifestData = await download.ConfigureAwait(false);
            }

            return new FileManifest(fileManifestData, remapExtraContent);
        }
    }
}
