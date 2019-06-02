using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    static class Program
    {
        public static RegistryKey MainRegistry = Registry.CurrentUser.GetSubKey("SOFTWARE", "Roblox Studio Mod Manager");
        public static RegistryKey VersionRegistry = MainRegistry.GetSubKey("VersionData");

        private const string _ = ""; // Default key/value used for stuff in UpdateStudioRegistryProtocols.

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

        public static void SetValue(string name, object value)
        {
            MainRegistry.SetValue(name, value);
        }
        
        // This sets up the following:
        // 1: The File Protocol to open .rbxl/.rbxlx files using my mod manager.
        // 2: The URI Protcol to open places from the website through my mod manager.

        public static void UpdateStudioRegistryProtocols()
        {
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
                GetSubKey(classes, ".rbxl"),
                GetSubKey(classes, ".rbxlx")
            };

            foreach (RegistryKey rbxLevel in robloxLevelPass)
            {
                rbxLevel.SetValue(_, "Roblox.Place");
                GetSubKey(rbxLevel, "Roblox.Place", "ShellNew");
                rbxLevel.Close();
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

        private static bool validateCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return (errors == SslPolicyErrors.None);
        }

        [STAThread]
        static void Main(string[] args)
        {
            // Delete deprecated startup protocol if it exists.
            try
            {
                bool registryInit = GetBool("Initialized Startup Protocol");

                if (registryInit)
                {
                    string myPath = Application.ExecutablePath;

                    RegistryKey startUpBin = Registry.CurrentUser.GetSubKey("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Run");
                    startUpBin.DeleteValue("RobloxStudioModManager");

                    SetValue("Initialized Startup Protocol", false);
                }
            }
            catch
            {
                Console.WriteLine("Ran into problem while removing deprecated startup protocol. Ignoring for now?");
            }

            // Add Roblox HTTPS validation 
            var httpsValidator = new RemoteCertificateValidationCallback(validateCert);
            ServicePointManager.ServerCertificateValidationCallback += httpsValidator;

            // Start launcher
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Launcher(args));
        }
    }
}
