using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using RobloxDeployHistory;

namespace RobloxStudioModManager
{
    internal class PackageManifest : List<Package>
    {
        public string RawData { get; private set; }

        private PackageManifest(string data)
        {
            using (var reader = new StringReader(data))
            {
                string version = reader.ReadLine();

                if (version != "v0")
                {
                    string errorMsg = $"Unexpected package manifest version: {version} (expected v0!)\n" +
                                       "Please contact MaximumADHD if you see this error.";

                    throw new NotSupportedException(errorMsg);
                }

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
                    string fileName = readLine();
                    string signature = readLine();

                    string rawPackedSize = readLine();
                    string rawSize = readLine();

                    if (eof)
                        break;

                    if (!int.TryParse(rawPackedSize, out int packedSize))
                        break;

                    if (!int.TryParse(rawSize, out int size))
                        break;

                    var package = new Package()
                    {
                        Name = fileName,
                        Signature = signature,
                        PackedSize = packedSize,
                        Size = size
                    };

                    Add(package);
                }
            }
            
            RawData = data;
        }

        public static async Task<PackageManifest> Get(ClientVersionInfo info)
        {
            Channel channel = info.Channel;
            string versionGuid = info.VersionGuid;

            string pkgManifestUrl = $"https://setup.rbxcdn.com/channel/{channel}/{versionGuid}-rbxPkgManifest.txt";
            string pkgManifestData;

            using (WebClient http = new WebClient())
            {
                var getData = http.DownloadStringTaskAsync(pkgManifestUrl);
                pkgManifestData = await getData.ConfigureAwait(false);
            }

            return new PackageManifest(pkgManifestData);
        }
    }
}
