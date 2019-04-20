using System;
using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class ClientVersionInfo
    {
        public string Version;
        public string Guid;

        public static async Task<ClientVersionInfo> Get(string buildType = "WindowsStudio", string branch = "roblox")
        {
            string jsonUrl = $"https://clientsettings.{branch}.com/v1/client-version/{buildType}";

            using (WebClient http = new WebClient())
            {
                string jsonData = await http.DownloadStringTaskAsync(jsonUrl);
                jsonData = jsonData.Replace('{', ' ');

                if (jsonData.Contains("bootstrap"))
                {
                    int boot = jsonData.IndexOf("bootstrap");
                    jsonData = jsonData.Substring(0, boot - 2);
                }
                else
                {
                    jsonData = jsonData.Replace('}', ' ');
                }

                var versionInfo = new ClientVersionInfo();
                jsonData = jsonData.Trim();

                foreach (string kvPairStr in jsonData.Split(','))
                {
                    string[] kvPair = kvPairStr.Split(':');

                    string key = kvPair[0]
                        .Replace('"', ' ')
                        .Trim();

                    string val = kvPair[1]
                        .Replace('"', ' ')
                        .Trim();

                    if (key == "version")
                    {
                        versionInfo.Version = val;
                        continue;
                    }
                    else if (key == "clientVersionUpload")
                    {
                        versionInfo.Guid = val;
                        continue;
                    }

                    Console.WriteLine("Unhandled key: {0}?", key);
                }

                return versionInfo;
            }
        }
    }
}
