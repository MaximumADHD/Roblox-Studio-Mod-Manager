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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

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
            new RbxInstallProtocol("LibrariesQt5"),
            new RbxInstallProtocol("redist"),

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
            new RbxInstallProtocol("Qml",                  @"Qml"),
            new RbxInstallProtocol("Plugins",              @"Plugins")
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

            RegistryKey studioQT = createSubKey(Registry.CurrentUser, "SOFTWARE","StudioQTRobloxReg");
            studioQT.SetValue(_, modManagerPath);
            studioQT.SetValue("protocol handler scheme", "roblox-studio");
            studioQT.SetValue("install host", setupDir);
            studioQT.SetValue("version", buildName);

            RegistryKey versions = createSubKey(studioQT, "Versions");
            versions.SetValue("version0", buildName);

            // Register the base "Roblox.Place" open protocol.
            RegistryKey classes = createSubKey(Registry.CurrentUser, "SOFTWARE","Classes");
            RegistryKey robloxPlace = createSubKey(classes,"Roblox.Place");
            robloxPlace.SetValue(_,"Roblox Place");

            RegistryKey robloxStudioUrl = createSubKey(classes,"roblox-studio");
            robloxStudioUrl.SetValue(_, "URL: Roblox Protocol");
            robloxStudioUrl.SetValue("URL Protocol", _);

            RegistryKey[] appReg = { robloxPlace, robloxStudioUrl };
            foreach (RegistryKey app in appReg)
            {
                RegistryKey defaultIcon = createSubKey(app, "DefaultIcon");
                defaultIcon.SetValue(_, robloxStudioBetaPath + ",0");
                defaultIcon.Close();
                RegistryKey command = createSubKey(app, "shell", "open", "command");
                command.SetValue(_, "\"" + robloxStudioBetaPath + "\" %1");
                command.SetValue(_, modManagerPath + " \"%1\"");
                app.Close();
            }

            // Pass the .rbxl and .rbxlx file formats to Roblox.Place
            RegistryKey rbxl = createSubKey(classes, ".rbxl");
            RegistryKey rbxlx = createSubKey(classes, ".rbxlx");
            RegistryKey[] robloxLevelPass = { rbxl, rbxlx };

            foreach (RegistryKey rbxLevel in robloxLevelPass)
            {
                rbxLevel.SetValue("", "Roblox.Place");
                createSubKey(rbxLevel, "Roblox.Place", "ShellNew");
                rbxLevel.Close();
            }
        }

        public async Task<string> RunInstaller(string database, bool forceInstall)
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
            http.UseDefaultCredentials = true;

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string rootDir = getDirectory(localAppData, "Roblox Studio");
            string downloads = getDirectory(rootDir, "downloads");

            Show();
            BringToFront();

            await setStatus("Checking for updates");

            setupDir = "setup." + database + ".com/";
            buildName = await http.DownloadStringTaskAsync("http://" + setupDir + "versionQTStudio");
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
                    string zipFileUrl = "https://" + setupDir + buildName + "-" + protocol.FileName + ".zip";
                    string extractDir = getDirectory(buildPath, protocol.LocalDirectory);
                    Console.WriteLine(extractDir);
                    string downloadPath = Path.Combine(downloads, protocol.FileName + ".zip");
                    Console.WriteLine(downloadPath);
                    await echo("Fetching: " + zipFileUrl);
                    try
                    {
                        byte[] downloadedFile = await http.DownloadDataTaskAsync(zipFileUrl);
                        FileStream writeFile = File.Create(downloadPath);
                        writeFile.Write(downloadedFile, 0, downloadedFile.Length);
                        writeFile.Close();
                        ZipArchive archive = ZipFile.Open(downloadPath, ZipArchiveMode.Read);
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                string localPath = Path.Combine(protocol.LocalDirectory, entry.FullName);
                                string path = Path.Combine(buildPath, localPath);
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
                    }
                    catch
                    {
                        await echo("Something went wrong while installing" + zipFileUrl);
                    }
                    progressBar.Increment(30);
                }
                await setStatus("Configuring Roblox Studio");
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