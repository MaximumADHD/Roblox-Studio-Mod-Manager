using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1031 // Do not catch general exception types

namespace RobloxStudioModManager
{
    public class Package
    {
        public string Name      { get; set; }
        public string Signature { get; set; }
        public int PackedSize   { get; set; }
        public int Size         { get; set; }
    }

    public class PackageManifest : List<Package>
    {
        private PackageManifest(string data)
        {
            using (StringReader reader = new StringReader(data))
            {
                string version = reader.ReadLine();

                if (version != "v0")
                {
                    string errorMsg = $"Unexpected package manifest version: {version} (expected v0!)\n" +
                                       "Please contact CloneTrooper1019 if you see this error.";

                    throw new NotSupportedException(errorMsg);
                }

                while (true)
                {
                    try
                    {
                        string fileName = reader.ReadLine();
                        string signature = reader.ReadLine();

                        int packedSize = int.Parse(reader.ReadLine(), Program.NumberFormat);
                        int size = int.Parse(reader.ReadLine(), Program.NumberFormat);

                        if (fileName.EndsWith(".zip", Program.StringFormat))
                        {
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
                    catch
                    {
                        break;
                    }
                }
            }
        }

        public static async Task<PackageManifest> Get(string branch, string versionGuid)
        {
            string pkgManifestUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{versionGuid}-rbxPkgManifest.txt";
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
