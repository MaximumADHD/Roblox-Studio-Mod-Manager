using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Microsoft.Win32;
#pragma warning disable CA2000 // Dispose objects before losing scope

namespace RobloxStudioModManager
{
    static class Program
    {
        [DllImport("Shcore.dll")]
        static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);
        
        public static readonly RegistryKey MainRegistry = Registry.CurrentUser.GetSubKey("SOFTWARE", "Roblox Studio Mod Manager");
        public static RegistryKey VersionRegistry => GetProfileRegistry("VersionData");

        private static readonly string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
        private static readonly string rootStudioDir = Path.Combine(localAppData, "Roblox Studio");

        public static readonly CultureInfo Format = CultureInfo.InvariantCulture;
        public const StringComparison StringFormat = StringComparison.InvariantCulture;
        public static readonly NumberFormatInfo NumberFormat = NumberFormatInfo.InvariantInfo;

        private static readonly Regex jsonPattern = new Regex("\"([^\"]*)\":\"?([^\"]*)\"?[,|}]");

        public static RegistryKey GetSubKey(this RegistryKey key, params string[] path)
        {
            string constructedPath = Path.Combine(path);
            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }

        public static string GetString(this RegistryKey key, string name, string fallback = "")
        {
            var result = key.GetValue(name, fallback);
            return result.ToString();
        }

        public static bool GetBool(this RegistryKey key, string name)
        {
            string value = key.GetString(name);

            if (!bool.TryParse(value, out bool result))
                return false;

            return result;
        }

        public static int GetInt(this RegistryKey key, string name)
        {
            string value = key.GetString(name);

            if (!int.TryParse(value, out int result))
                return 0;

            return result;
        }

        public static RegistryKey GetSubKey(params string[] path)
        {
            return MainRegistry.GetSubKey(path);
        }

        public static string GetString(string name, string fallback = "")
        {
            return MainRegistry.GetString(name, fallback);
        }

        public static bool GetBool(string name)
        {
            return MainRegistry.GetBool(name);
        }

        public static int GetInt(string name)
        {
            return MainRegistry.GetInt(name);
        }

        public static void SetValue(string name, object value)
        {
            MainRegistry.SetValue(name, value);
        }

        public static RegistryKey GetProfileRegistry(params string[] path)
        {
            string studioDir = MainRegistry.GetString("BuildDirectory");

            if (string.IsNullOrEmpty(studioDir))
                studioDir = rootStudioDir;
            
            if (!Directory.Exists(studioDir))
                Directory.CreateDirectory(studioDir);

            RegistryKey result = MainRegistry;

            if (studioDir != rootStudioDir)
            {
                var profiles = result.GetSubKey("Profiles");
                string key = studioDir.Replace('\\', '/');
                result = profiles.GetSubKey(key);
            }

            return result.GetSubKey(path);
        }

        public static Dictionary<string, string> ReadJsonDictionary(string json)
        {
            var matches = jsonPattern.Matches(json);
            var result = new Dictionary<string, string>();

            foreach (Match match in matches)
            {
                var data = match.Groups
                    .Cast<Group>()
                    .Select(g => g.Value)
                    .ToArray();

                string key = data[1],
                       val = data[2];

                result.Add(key, val);
            }

            return result;
        }

        // This sets up the following:
        // 1: The File Protocol to open .rbxl/.rbxlx files using my mod manager.
        // 2: The URI Protcol to open places from the website through my mod manager.

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

        static Program()
        {
            const int SYSTEM_AWARE = 1;
            _ = SetProcessDpiAwareness(SYSTEM_AWARE);
        }

        [STAThread]
        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Launcher(args));
        }
    }
}
