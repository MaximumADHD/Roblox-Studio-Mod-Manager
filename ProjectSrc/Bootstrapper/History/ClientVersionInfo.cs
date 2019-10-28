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

                var json = Program.ReadJsonDictionary(jsonData);
                var versionInfo = new ClientVersionInfo();

                json.TryGetValue("version", out versionInfo.Version);
                json.TryGetValue("clientVersionUpload", out versionInfo.Guid);

                return versionInfo;
            }
        }
    }
}
