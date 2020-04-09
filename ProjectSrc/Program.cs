using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    static class Program
    {
        public static RegistryKey MainRegistry = Registry.CurrentUser.GetSubKey("SOFTWARE", "Roblox Studio Mod Manager");
        public static RegistryKey VersionRegistry = MainRegistry.GetSubKey("VersionData");

        private static Regex jsonPattern = new Regex("\"([^\"]*)\":\"?([^\"]*)\"?[,|}]");

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
            bool result = false;

            bool.TryParse(value, out result);
            return result;
        }

        public static int GetInt(this RegistryKey key, string name)
        {
            string value = key.GetString(name);
            int result = 0;

            int.TryParse(value, out result);
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
            string modManagerPath = Application.ExecutablePath;

            // Register the base "Roblox.Place" open protocol.
            RegistryKey classes = Registry.CurrentUser.GetSubKey("SOFTWARE", "Classes");

            RegistryKey robloxPlace = classes.GetSubKey("Roblox.Place");
            robloxPlace.SetValue(_, "Roblox Place");

            RegistryKey robloxPlaceCmd = robloxPlace.GetSubKey("shell", "open", "command");
            robloxPlaceCmd.SetValue(_, $"\"{modManagerPath}\"\" -task EditFile -localPlaceFile \"%1\"");

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
