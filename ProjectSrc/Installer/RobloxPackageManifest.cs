using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class RobloxPackageManifest
    {
        public string Name;
        public string Signature;
        public int PackedSize;
        public int Size;

        public static async Task<List<RobloxPackageManifest>> Get(string branch, string versionGuid)
        {
            var result = new List<RobloxPackageManifest>();
            
            string pkgManifestUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{versionGuid}-rbxPkgManifest.txt";
            string pkgManifestData;

            using (WebClient http = new WebClient())
                pkgManifestData = await http.DownloadStringTaskAsync(pkgManifestUrl);

            using (StringReader reader = new StringReader(pkgManifestData))
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

                        int packedSize = int.Parse(reader.ReadLine());
                        int size = int.Parse(reader.ReadLine());

                        if (fileName.EndsWith(".zip"))
                        {
                            var pkgManifest = new RobloxPackageManifest()
                            {
                                Name = fileName,
                                Signature = signature,
                                PackedSize = packedSize,
                                Size = size
                            };

                            result.Add(pkgManifest);
                        }
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
