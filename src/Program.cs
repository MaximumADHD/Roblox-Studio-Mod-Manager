using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    static class Program
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static RegistryKey ModManagerRegistry = GetSubKey(Registry.CurrentUser, "SOFTWARE", "Roblox Studio Mod Manager");
        private static string _ = ""; // Default key/value used for stuff in UpdateStudioRegistryProtocols.

        public static RegistryKey GetSubKey(RegistryKey key, params string[] path)
        {
            string constructedPath = "";
            foreach (string p in path)
                constructedPath = Path.Combine(constructedPath, p);

            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }

        // This sets up the following:
        // 1: The File Protocol to open .rbxl/.rbxlx files using my mod manager.
        // 2: The URI Protcol to open places from the website through my mod manager.
        // 3: A bunch of other misc(?) things that Roblox might check for.
        public static void UpdateStudioRegistryProtocols(string setupDir, string buildName, string robloxStudioBetaPath)
        {
            string modManagerPath = Application.ExecutablePath;

            RegistryKey studioQT = GetSubKey(Registry.CurrentUser, "SOFTWARE", "StudioQTRobloxReg");
            studioQT.SetValue(_, modManagerPath);
            studioQT.SetValue("protocol handler scheme", "roblox-studio");
            studioQT.SetValue("install host", setupDir);
            studioQT.SetValue("version", buildName);

            RegistryKey versions = GetSubKey(studioQT, "Versions");
            versions.SetValue("version0", buildName);

            // Register the base "Roblox.Place" open protocol.
            RegistryKey classes = GetSubKey(Registry.CurrentUser, "SOFTWARE", "Classes");
            RegistryKey robloxPlace = GetSubKey(classes, "Roblox.Place");
            robloxPlace.SetValue(_, "Roblox Place");

            RegistryKey robloxStudioUrl = GetSubKey(classes, "roblox-studio");
            robloxStudioUrl.SetValue(_, "URL: Roblox Protocol");
            robloxStudioUrl.SetValue("URL Protocol", _);

            RegistryKey[] appReg = { robloxPlace, robloxStudioUrl };
            foreach (RegistryKey app in appReg)
            {
                RegistryKey defaultIcon = GetSubKey(app, "DefaultIcon");
                defaultIcon.SetValue(_, robloxStudioBetaPath + ",0");
                defaultIcon.Close();
                RegistryKey command = GetSubKey(app, "shell", "open", "command");
                command.SetValue(_, "\"" + robloxStudioBetaPath + "\" %1");
                command.SetValue(_, modManagerPath + " \"%1\"");
                app.Close();
            }

            // Pass the .rbxl and .rbxlx file formats to Roblox.Place
            RegistryKey rbxl = GetSubKey(classes, ".rbxl");
            RegistryKey rbxlx = GetSubKey(classes, ".rbxlx");
            RegistryKey[] robloxLevelPass = { rbxl, rbxlx };

            foreach (RegistryKey rbxLevel in robloxLevelPass)
            {
                rbxLevel.SetValue(_, "Roblox.Place");
                GetSubKey(rbxLevel, "Roblox.Place", "ShellNew");
                rbxLevel.Close();
            }
        }

        private static string playerVersion;
        private static RegistryKey roblox;

        private static void checkForClientUpdate(object sender, ElapsedEventArgs e)
        {
            string currentVersion = roblox.GetValue("version", "") as string;
            if (playerVersion != currentVersion)
            {
                string buildDatabase = ModManagerRegistry.GetValue("BuildDatabase", "") as string;
                string buildVersion = ModManagerRegistry.GetValue("BuildVersion", "") as string;
                if (buildDatabase.Length > 0 && buildVersion.Length > 0)
                {
                    string setupDir = "https://setup." + buildDatabase + ".com/";
                    string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
                    string robloxStudioBetaPath = Path.Combine(localAppData, "Roblox Studio", "RobloxStudioBeta.exe");
                    UpdateStudioRegistryProtocols(setupDir, buildVersion, robloxStudioBetaPath);
                    playerVersion = currentVersion;
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            // START-UP PRE-REGISTRY
            // Theres an issue with Roblox's regular player that makes it overwrite my launcher's URI protocol with theirs when a Roblox update happens.
            // Unfortunately the only way I can work around this is if I have the program running in the background with a special mode that checks for this.
            // Special thanks to LucasWolschick for bringing this to my attention.
            bool registryInit = bool.Parse(ModManagerRegistry.GetValue("Initialized Startup Protocol", "False") as string);
            if (!registryInit)
            {
                string myPath = Application.ExecutablePath;
                RegistryKey startUpBin = GetSubKey(Registry.CurrentUser, "SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Run");
                startUpBin.SetValue("RobloxStudioModManager", '"' + myPath + "\" -UpdateWatcherMode");
                ModManagerRegistry.SetValue("Initialized Startup Protocol", true);
                // Hook up the background watcher now since it wasn't started when Windows started.
                Process.Start(myPath,"-UpdateWatcherMode");
            }

            if (args.Length > 0 && args[0] == "-UpdateWatcherMode")
            {
                roblox = GetSubKey(Registry.CurrentUser, "SOFTWARE", "RobloxReg");
                playerVersion = roblox.GetValue("version", "") as string;

                var timer = new System.Timers.Timer();
                timer.Interval = 3000;
                timer.AutoReset = true;
                timer.Elapsed += checkForClientUpdate;
                timer.Start();

                Application.Run();
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; // Unlocks https for the WebClient
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Launcher(args));
            }
        }
    }
}
