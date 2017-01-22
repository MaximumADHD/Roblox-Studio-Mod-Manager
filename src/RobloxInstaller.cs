using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxModManager
{
    public struct RbxInstallProtocol
    {
        public string FileName;
        public string LocalDirectory;

        public RbxInstallProtocol(string fileName, string localDirectory = "")
        {
            FileName = fileName;
            LocalDirectory = localDirectory;
        }
    }

    public struct RegistryKeyResult
    {
        public RegistryKey Result;
        public bool Completed;
    }

    public partial class RobloxInstaller : Form
    {
        private int echoSteps = 0;
        private static string _ = "";
        public string buildName;
        public string buildPath;
        public string setupDir;
        public string robloxStudioBetaPath;

        WebClient http = new WebClient();
        List<RbxInstallProtocol> instructions = new List<RbxInstallProtocol>()
        {
            new RbxInstallProtocol("RobloxStudio"),
            new RbxInstallProtocol("Libraries"),
            new RbxInstallProtocol("redist"),
            new RbxInstallProtocol("imageformats"),

            new RbxInstallProtocol("content-fonts",        @"content\fonts"),
            new RbxInstallProtocol("content-music",        @"content\music"),
            new RbxInstallProtocol("content-particles",    @"content\particles"),
            new RbxInstallProtocol("content-scripts",      @"content\scripts"),
            new RbxInstallProtocol("content-sky",          @"content\sky"),
            new RbxInstallProtocol("content-sounds",       @"content\sounds"),
            new RbxInstallProtocol("content-textures",     @"content\textures"),
            new RbxInstallProtocol("content-textures2",    @"content\textures"),

            new RbxInstallProtocol("content-terrain",      @"PlatformContent\pc\terrain"),
            new RbxInstallProtocol("content-textures3",    @"PlatformContent\pc\textures"),

            new RbxInstallProtocol("BuiltInPlugins",       @"BuiltInPlugins"),
            new RbxInstallProtocol("shaders",              @"shaders"),
        };

        public async Task setStatus(string status)
        {
            await Task.Delay(500);
            statusLbl.Text = status;
        }


        public async Task echo(string text)
        {
            echoSteps++;
            if (echoSteps % 20 == 0)
            {
                await Task.Delay(1);
            }
            log.Items.Add(text);
            log.SelectedIndex = log.Items.Count - 1;
            log.SelectedIndex = -1;
        }

        public RobloxInstaller()
        {
            InitializeComponent();
        }

        public string getDirectory(params string[] paths)
        {
            string basePath = "";
            foreach (string path in paths)
            {
                basePath = Path.Combine(basePath, path);
            }
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            return basePath;
        }

        public RegistryKey createSubKey(RegistryKey key, params string[] path)
        {
            string constructedPath = "";
            foreach (string p in path)
            {
                constructedPath = Path.Combine(constructedPath, p);
            }
            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }

        public RegistryKeyResult getRegistry(RegistryKey dir, params string[] traverse)
        {
            RegistryKeyResult result = new RegistryKeyResult();
            result.Completed = true;
            foreach (string subKey in traverse)
            {
                RegistryKey nextDir;
                try
                {
                    nextDir = createSubKey(dir,subKey);
                }
                catch
                {
                    result.Completed = false;
                    break;
                }
                if (nextDir != null)
                {
                    result.Result = nextDir;
                }
            }
            return result;
        }

        public void registryUpdate()
        {
            // Handle Roblox Studio Registry stuff.
            // Theres a lot here, bare with it.

            Assembly self = Assembly.GetExecutingAssembly();
            AssemblyName selfName = self.GetName();
            string modManagerDir = Path.GetDirectoryName(selfName.CodeBase).Substring(6);
            Process myProcess = Process.GetCurrentProcess();
            string processName = myProcess.ProcessName;
            string modManagerPath = Path.Combine(modManagerDir, processName + ".exe");
            bool isDebugging = processName.EndsWith(".vshost");

            RegistryKeyResult studioQTGet = getRegistry(Registry.CurrentUser, "Software", "StudioQTRobloxReg");
            if (studioQTGet.Completed)
            {
                // Standard Roblox Studio registration (for whatever it needs this for)
                RegistryKey studioQT = studioQTGet.Result;
                if (!isDebugging)
                {
                    studioQT.SetValue(_, modManagerPath);
                }
                studioQT.SetValue("protocol handler scheme", "roblox-studio");
                studioQT.SetValue("install host", setupDir);
                studioQT.SetValue("version", buildName);

                RegistryKey versions = createSubKey(studioQT, "Versions");
                versions.SetValue("version0", buildName);

                Registry.CurrentUser.Close();
            }

            if (!isDebugging)
            {
                // Register the base "Roblox.Place" open protocol.
                RegistryKeyResult robloxPlaceGet = getRegistry(Registry.ClassesRoot, "Roblox.Place");
                RegistryKey robloxPlace = robloxPlaceGet.Result;
                if (robloxPlaceGet.Completed)
                {
                    robloxPlace.SetValue(_, "Roblox Place");
                }

                // Register the url protocol
                RegistryKeyResult robloxStudioUrlGet = getRegistry(Registry.ClassesRoot, "roblox-studio");
                RegistryKey robloxStudioUrl = robloxStudioUrlGet.Result;
                if (robloxStudioUrlGet.Completed)
                {
                    robloxStudioUrl.SetValue(_, "URL: Roblox Protocol");
                    robloxStudioUrl.SetValue("URL Protocol", "");
                }

                // Register both of these protocols to open with the mod manager.
                RegistryKeyResult[] appGetReg = { robloxPlaceGet, robloxStudioUrlGet };
                foreach (RegistryKeyResult appGet in appGetReg)
                {
                    if (appGet.Completed)
                    {
                        RegistryKey app = appGet.Result;
                        RegistryKey defaultIcon = createSubKey(app, "DefaultIcon");
                        defaultIcon.SetValue(_, robloxStudioBetaPath + ",0");
                        defaultIcon.Close();
                        RegistryKey command = createSubKey(app, "shell", "open", "command");
                        command.SetValue(_, "\"" + robloxStudioBetaPath + "\" %1");
                        command.SetValue(_, modManagerPath + " \"%1\"");
                        app.Close();
                    }
                }

                // Pass the .rbxl and .rbxlx file formats to Roblox.Place
                RegistryKeyResult rbxlGet = getRegistry(Registry.ClassesRoot, ".rbxl");
                RegistryKeyResult rbxlxGet = getRegistry(Registry.ClassesRoot, ".rbxlx");
                RegistryKeyResult[] robloxLevelPass = { rbxlGet, rbxlxGet };

                foreach (RegistryKeyResult rbxLevelGet in robloxLevelPass)
                {
                    if (rbxLevelGet.Completed)
                    {
                        RegistryKey rbxLevel = rbxLevelGet.Result;
                        rbxLevel.SetValue("", "Roblox.Place");
                        createSubKey(rbxLevel, "Roblox.Place", "ShellNew");
                        rbxLevel.Close();
                    }
                }
            }
        }

        public async Task<string> RunInstaller(string database, bool forceInstall)
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string rootDir = getDirectory(localAppData, "Roblox Studio");
            string downloads = getDirectory(rootDir, "downloads");

            this.Show();
            this.BringToFront();

            await setStatus("Checking for updates");

            setupDir = "http://setup." + database + ".com/";
            buildName = await http.DownloadStringTaskAsync(setupDir + "versionQTStudio");
            buildPath = getDirectory(rootDir, buildName);
            robloxStudioBetaPath = Path.Combine(buildPath, "RobloxStudioBeta.exe");

            string appSettings = Path.Combine(buildPath, "AppSettings.xml");
            await echo("Checking build installation...");

            if (!File.Exists(appSettings) || forceInstall)
            {
                await echo("This build hasn't been installed!");
                await setStatus("Loading latest Roblox Studio build from " + database + ".com");
                progressBar.Maximum = instructions.Count*30;
                progressBar.Value = 0;
                progressBar.Style = ProgressBarStyle.Continuous;
                foreach (RbxInstallProtocol protocol in instructions)
                {
                    string zipFileUrl = setupDir + buildName + "-" + protocol.FileName + ".zip";
                    string extractDir = getDirectory(buildPath, protocol.LocalDirectory);
                    Console.WriteLine(extractDir);
                    string downloadPath = Path.Combine(downloads, protocol.FileName + ".zip");
                    Console.WriteLine(downloadPath);
                    await echo("Fetching: " + zipFileUrl);
                    byte[] downloadedFile = await http.DownloadDataTaskAsync(zipFileUrl);
                    FileStream writeFile = File.Create(downloadPath);
                    writeFile.Write(downloadedFile,0,downloadedFile.Length);
                    writeFile.Close();
                    ZipArchive archive = ZipFile.Open(downloadPath, ZipArchiveMode.Read);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            string localPath = Path.Combine(protocol.LocalDirectory, entry.FullName);
                            string path = Path.Combine(buildPath,localPath);
                            await echo("Creating directory " + localPath);
                            getDirectory(path);
                        }
                        else
                        {
                            string entryName = entry.FullName;
                            await echo("Extracting " + entryName + " to " + buildName + "\\" + protocol.LocalDirectory);
                            string relativePath = Path.Combine(extractDir, entryName);
                            string directoryParent = Directory.GetParent(relativePath).ToString();
                            getDirectory(directoryParent);
                            if (!File.Exists(relativePath))
                            {
                                entry.ExtractToFile(relativePath);
                            }
                        }
                    }
                    progressBar.Increment(30);
                }
                await setStatus("Configuring ROBLOX Studio");
                progressBar.Style = ProgressBarStyle.Marquee;
                await echo("Writing AppSettings.xml");
                File.WriteAllText(appSettings, "<Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>");
                await echo("Roblox Studio " + buildName + " successfully installed!");
            }
            else
            {
                await echo("This version of Roblox Studio has been installed!");
            }

            await setStatus("Configuring Roblox Studio...");
            registryUpdate();

            await setStatus("Starting Roblox Studio...");
            return robloxStudioBetaPath;
        }
    }
}