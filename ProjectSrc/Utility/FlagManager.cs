using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace RobloxStudioModManager
{
    // Agnostic flag application logic.
    public static class FlagManager
    {
        public static void ApplyFlags()
        {
            var json = new JObject();
            var flagRegistry = App.State.FlagEditor;

            foreach (string flagName in flagRegistry.Keys)
            {
                var flag = flagRegistry[flagName];
                string value = flag.Value;

                var token = JToken.FromObject(value);
                json.Add(flagName, token);
            };

            string file = json.ToString();
            string studioDir = StudioBootstrapper.GetStudioDirectory();
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");

            string clientSettings_DEPRECATED = Path.Combine(studioDir, "ClientSettings");
            Directory.CreateDirectory(clientSettings_DEPRECATED);

            string filePath_DEPRECATED = Path.Combine(clientSettings_DEPRECATED, "ClientAppSettings.json");
            File.WriteAllText(filePath_DEPRECATED, file);

            string clientSettings_NEW = Path.Combine(localAppData, "Roblox", "ClientSettings");
            Directory.CreateDirectory(clientSettings_NEW);

            string filePath_NEW = Path.Combine(clientSettings_NEW, "ClientAppSettings.json");
            File.WriteAllText(filePath_NEW, file);
        }
    }
}
