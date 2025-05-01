using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Win32;
using System.Text;

namespace RobloxStudioModManager
{
    static class Program
    {
        public const string RepoBranch = "main";
        public const string RepoOwner = "MaximumADHD";
        public const string RepoName = "Roblox-Studio-Mod-Manager";

        public const string ReleaseTag = "v2025.05.01";
        public static readonly string BaseConfigUrl = $"https://raw.githubusercontent.com/{RepoOwner}/{RepoName}/{RepoBranch}/Config/";

        public static readonly RegistryKey LegacyRegistry = Registry.CurrentUser.GetSubKey("SOFTWARE", "Roblox Studio Mod Manager");
        public static readonly CultureInfo Format = CultureInfo.InvariantCulture;

        public const StringComparison StringFormat = StringComparison.Ordinal;
        public static readonly NumberFormatInfo NumberFormat = NumberFormatInfo.InvariantInfo;

        public static string RootDir { get; private set; }
        public static ModManagerState State { get; private set; }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new OrdinalSortJson()
        };

        public static RegistryKey GetSubKey(this RegistryKey key, params string[] path)
        {
            string constructedPath = Path.Combine(path);
            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }

        // This sets up the following:
        // 1: The File Protocol to open .rbxl/.rbxlx files through the mod manager.
        // 2: The URI Protcol to open places from the website through the mod manager.

        public static void UpdateStudioRegistryProtocols()
        {
            const string _ = ""; // Default empty key/value.

            string modManagerPath = Application.ExecutablePath
                .Replace('"', ' ')
                .Trim();

            // Register the base "Roblox.Place" open protocol.
            RegistryKey classes = Registry.CurrentUser.GetSubKey("SOFTWARE", "Classes");

            RegistryKey robloxPlace = classes.GetSubKey("Roblox.Place");
            robloxPlace.SetValue(_, "Roblox Place");

            RegistryKey robloxPlaceCmd = robloxPlace.GetSubKey("shell", "open", "command");
            robloxPlaceCmd.SetValue(_, $"\"{modManagerPath}\" -task EditFile -localPlaceFile \"%1\"");

            // Pass the .rbxl and .rbxlx file formats to Roblox.Place
            RegistryKey[] robloxLevelPass =
            {
                classes.GetSubKey(".rbxl"),
                classes.GetSubKey(".rbxlx")
            };

            foreach (RegistryKey rbxLevel in robloxLevelPass)
            {
                rbxLevel.SetValue(_, "Roblox.Place");
                rbxLevel.GetSubKey("Roblox.Place", "ShellNew");
            }

            // Setup the URI protocol for opening the mod manager through the website.
            RegistryKey robloxStudioUrl = GetSubKey(classes, "roblox-studio");
            robloxStudioUrl.SetValue(_, "URL: Roblox Protocol");
            robloxStudioUrl.SetValue("URL Protocol", _);

            RegistryKey studioUrlCmd = GetSubKey(robloxStudioUrl, "shell", "open", "command");
            studioUrlCmd.SetValue(_, modManagerPath + " %1");

            // Set the default icon for both protocols.
            RegistryKey[] appReg =
            {
                robloxPlace,
                robloxStudioUrl
            };

            foreach (RegistryKey app in appReg)
            {
                RegistryKey defaultIcon = GetSubKey(app, "DefaultIcon");
                defaultIcon.SetValue(_, $"{modManagerPath},0");
            }
        }

        static void ConvertLegacy(RegistryKey regKey, JObject node)
        {
            foreach (var name in regKey.GetValueNames())
            {
                string key = name.Replace(" ", "");
                object value = regKey.GetValue(name);

                var token = JToken.FromObject(value);
                node.Add(key, token);
            }

            foreach (var subKeyName in regKey.GetSubKeyNames())
            {
                var subKey = regKey.OpenSubKey(subKeyName);
                var child = new JObject();

                ConvertLegacy(subKey, child);
                node.Add(subKeyName, child);
            }
        }

        public static void SaveState()
        {
            var stateFile = Path.Combine(RootDir, "state.json");
            string json = JsonConvert.SerializeObject(State, Formatting.Indented, JsonSettings);
            File.WriteAllText(stateFile, json);
        }

        static void OnExiting(object sender, EventArgs e)
        {
            SaveState();
        }

        [STAThread]
        static void Main(string[] args)
        {
            // Initialize application state.
            var localAppData = Environment.GetEnvironmentVariable("localappdata");
            RootDir = Path.Combine(localAppData, "Roblox Studio Mod Manager");

            if (!Directory.Exists(RootDir))
                Directory.CreateDirectory(RootDir);

            var stateFile = Path.Combine(RootDir, "state.json");
            string json = "";

            if (File.Exists(stateFile))
                json = File.ReadAllText(stateFile);

            if (string.IsNullOrEmpty(json))
            {
                var root = new JObject();
                ConvertLegacy(LegacyRegistry, root);

                json = root.ToString();
                File.WriteAllText(stateFile, json);
            }

            State = JsonConvert.DeserializeObject<ModManagerState>(json);
            
            if (State == null)
                State = new ModManagerState();

            // Make sure HTTPS uses TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Standard windows form jank
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += new EventHandler(OnExiting);
            Application.Run(new Launcher(args));
        }
    }
}
