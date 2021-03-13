using System.Net;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public class ClientVersionInfo
    {
        public string Version { get; set; }
        public string VersionGuid { get; set; }

        public static async Task<ClientVersionInfo> Get(string buildType = "WindowsStudio", string branch = "roblox")
        {
            string jsonUrl = $"https://clientsettingscdn.{branch}.com/v1/client-version/{buildType}";

            using (WebClient http = new WebClient())
            {
                var getJsonData = http.DownloadStringTaskAsync(jsonUrl);
                string jsonData = await getJsonData.ConfigureAwait(false);

                var json = Program.ReadJsonDictionary(jsonData);
                var versionInfo = new ClientVersionInfo();

                if (json.TryGetValue("version", out string version))
                    versionInfo.Version = version;

                if (json.TryGetValue("clientVersionUpload", out string versionGuid))
                    versionInfo.VersionGuid = versionGuid;

                return versionInfo;
            }
        }
    }
}
