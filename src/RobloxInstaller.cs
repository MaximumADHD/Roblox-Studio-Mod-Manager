using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
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
        private static string _ = "";
        public string buildName;
        public string setupDir;
        public string robloxStudioBetaPath;

        public delegate void EchoDelegator(string text);
        public delegate void IncrementDelegator(int count);

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


        private void echo(string text)
        {
            if (InvokeRequired)
                Invoke(new EchoDelegator(echo), text);
            else
            {
                if (log.Text != "")
                    log.AppendText("\n");

                log.AppendText(text);
            }
        }

        private void incrementProgress()
        {
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new IncrementDelegator(progressBar.Increment), 30);
            else
                progressBar.Increment(30);
        }

        public RobloxInstaller()
        {
            InitializeComponent();
        }

        private static string getDirectory(params string[] paths)
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

        private static RegistryKey createSubKey(RegistryKey key, params string[] path)
        {
            string constructedPath = "";
            foreach (string p in path)
            {
                constructedPath = Path.Combine(constructedPath, p);
            }
            return key.CreateSubKey(constructedPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
        }

        private static RegistryKeyResult getRegistry(RegistryKey dir, params string[] traverse)
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
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; // Unlocks https for the WebClient

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string rootDir = getDirectory(localAppData, "Roblox Studio");
            string downloads = getDirectory(rootDir, "downloads");

            RegistryKey managerRegistry = createSubKey(Registry.CurrentUser, "SOFTWARE", "Roblox Studio Mod Manager");
            RegistryKey checkSum = createSubKey(managerRegistry, "Checksum");

            Show();
            BringToFront();

            await setStatus("Checking for updates");

            setupDir = "setup." + database + ".com/";
            buildName = await http.DownloadStringTaskAsync("http://" + setupDir + "versionQTStudio");
            robloxStudioBetaPath = Path.Combine(rootDir, "RobloxStudioBeta.exe");

            echo("Checking build installation...");

            string currentBuildDatabase = managerRegistry.GetValue("BuildDatabase","") as string;
            string currentBuildVersion = managerRegistry.GetValue("BuildVersion", "") as string;

            if (currentBuildDatabase != database || currentBuildVersion != buildName || forceInstall)
            {
                echo("This build needs to be installed!");
                await setStatus("Loading latest Roblox Studio build from " + database + ".com");
                progressBar.Maximum = instructions.Count*30;
                progressBar.Value = 0;
                progressBar.Style = ProgressBarStyle.Continuous;

                bool safeToContinue = false;
                bool cancelled = false;
                int attempts = 0;

                while (!safeToContinue)
                {
                    Process[] running = Process.GetProcessesByName("RobloxStudioBeta");
                    if (running.Length > 0)
                    {
                        foreach (Process p in running)
                            p.CloseMainWindow();

                        attempts++;
                        await Task.Delay(1);

                        if (attempts > 50)
                        {
                            echo("Running apps detected, action on the user's part is needed.");
                            attempts = 0;
                            Hide();
                            DialogResult result = MessageBox.Show("All Roblox Studio instances needs to be closed in order to install the new version.\nPress Ok once you've saved your work and the windows are closed, or\nPress Cancel to skip the update for this launch.", "Notice", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            Show();
                            if (result == DialogResult.Cancel)
                            {
                                safeToContinue = true;
                                cancelled = true;
                            }
                        }
                    }
                    else
                        safeToContinue = true;
                }

                if (!cancelled)
                {
                    List<Task> taskQueue = new List<Task>();

                    foreach (RbxInstallProtocol protocol in instructions)
                    {
                        Task installer = Task.Run(async () =>
                        {
                            string zipFileUrl = "https://" + setupDir + buildName + "-" + protocol.FileName + ".zip";
                            string extractDir = getDirectory(rootDir, protocol.LocalDirectory);
                            Console.WriteLine(extractDir);
                            string downloadPath = Path.Combine(downloads, protocol.FileName + ".zip");
                            Console.WriteLine(downloadPath);
                            echo("Fetching: " + zipFileUrl);
                            try
                            {
                                WebClient localHttp = new WebClient();
                                localHttp.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
                                byte[] downloadedFile = await localHttp.DownloadDataTaskAsync(zipFileUrl);
                                bool doInstall = true;
                                if (!forceInstall)
                                {
                                    string fileHash = downloadedFile.Length.ToString(); // Cheap and probably won't work that well, but it's pretty slow to do an actual hash check.
                                    string currentHash = checkSum.GetValue(protocol.FileName, "") as string;
                                    if (fileHash == currentHash)
                                    {
                                        echo(protocol.FileName + ".zip hasn't changed between builds, skipping.");
                                        doInstall = false;
                                    }
                                    else checkSum.SetValue(protocol.FileName, fileHash);
                                }
                                if (doInstall)
                                {
                                    FileStream writeFile = File.Create(downloadPath);
                                    writeFile.Write(downloadedFile, 0, downloadedFile.Length);
                                    writeFile.Close();
                                    ZipArchive archive = ZipFile.Open(downloadPath, ZipArchiveMode.Read);
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        if (string.IsNullOrEmpty(entry.Name))
                                        {
                                            string localPath = Path.Combine(protocol.LocalDirectory, entry.FullName);
                                            string path = Path.Combine(rootDir, localPath);
                                            echo("Creating directory " + localPath);
                                            getDirectory(path);
                                        }
                                        else
                                        {
                                            string entryName = entry.FullName;
                                            echo("Extracting " + entryName + " to " + buildName + "\\" + protocol.LocalDirectory);
                                            string relativePath = Path.Combine(extractDir, entryName);
                                            string directoryParent = Directory.GetParent(relativePath).ToString();
                                            getDirectory(directoryParent);
                                            if (!File.Exists(relativePath))
                                                entry.ExtractToFile(relativePath);
                                        }
                                    }
                                }
                                localHttp.Dispose();
                            }
                            catch
                            {
                                echo("Something went wrong while installing. This build may not install correctly. File: " + zipFileUrl);
                            }
                            incrementProgress();
                        });
                        taskQueue.Add(installer);
                    }
                    await Task.WhenAll(taskQueue.ToArray());

                    await setStatus("Configuring Roblox Studio");
                    progressBar.Style = ProgressBarStyle.Marquee;
                    echo("Writing AppSettings.xml");

                    string appSettings = Path.Combine(rootDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, "<Settings><ContentFolder>content</ContentFolder><BaseUrl>http://www.roblox.com</BaseUrl></Settings>");

                    echo("Roblox Studio " + buildName + " successfully installed!");
                }
                else
                {
                    echo("Update cancelled. Proceeding with launch on current database and version.");
                }
            }
            else
            {
                echo("This version of Roblox Studio has been installed!");
            }

            await setStatus("Configuring Roblox Studio...");
            managerRegistry.SetValue("BuildDatabase", database);
            managerRegistry.SetValue("BuildVersion", buildName);
            registryUpdate();

            await setStatus("Starting Roblox Studio...");
            return robloxStudioBetaPath;
        }

        private void RobloxInstaller_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}