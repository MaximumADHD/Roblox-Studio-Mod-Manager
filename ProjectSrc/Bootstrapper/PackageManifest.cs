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
            using (StringReader reader = new StringReader(data))
            {
                string version = reader.ReadLine();

                if (version != "v0")
                    throw new NotSupportedException($"Unexpected package manifest version: {version} (expected v0!)");

                while (true)
                {
                    string fileName = reader.ReadLine();
                    string signature = reader.ReadLine();

                    string rawPackedSize = reader.ReadLine();
                    string rawSize = reader.ReadLine();

                    if (string.IsNullOrEmpty(fileName) ||
                        string.IsNullOrEmpty(signature) ||
                        string.IsNullOrEmpty(rawPackedSize) ||
                        string.IsNullOrEmpty(rawSize))
                        break;

                    // ignore launcher
                    if (fileName == "RobloxPlayerLauncher.exe")
                        break;

                    int packedSize = int.Parse(rawPackedSize);
                    int size = int.Parse(rawSize);

                    Add(new Package
                    {
                        Name = fileName,
                        Signature = signature,
                        PackedSize = packedSize,
                        Size = size
                    });
                }
            }

            RawData = data;
        }

        public static async Task<PackageManifest> Get(ClientVersionInfo info)
        {
            Channel channel = info.Channel;
            string versionGuid = info.VersionGuid;

            string pkgManifestUrl = $"{channel.BaseUrl}/{versionGuid}-rbxPkgManifest.txt";
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
