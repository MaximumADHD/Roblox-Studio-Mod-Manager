using System.Net;
using System.Threading.Tasks;

#pragma warning disable CA1720 // Identifier contains type name

namespace RobloxStudioModManager
{
    public class ClientVersionInfo
    {
        private string RawVersion;
        private string RawGuid;

        public string Version
        {
            get => RawVersion;
            set => RawVersion = value;
        }

        public string Guid
        {
            get => RawGuid;
            set => RawGuid = value;
        }

        public static async Task<ClientVersionInfo> Get(string buildType = "WindowsStudio", string branch = "roblox")
        {
            string jsonUrl = $"https://clientsettingscdn.{branch}.com/v1/client-version/{buildType}";

            using (WebClient http = new WebClient())
            {
                var getJsonData = http.DownloadStringTaskAsync(jsonUrl);
                string jsonData = await getJsonData.ConfigureAwait(false); 

                var json = Program.ReadJsonDictionary(jsonData);
                var versionInfo = new ClientVersionInfo();

                json.TryGetValue("version", out versionInfo.RawVersion);
                json.TryGetValue("clientVersionUpload", out versionInfo.RawGuid);

                return versionInfo;
            }
        }
    }
}
