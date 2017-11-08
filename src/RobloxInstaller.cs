using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxModManager
{
    public struct RbxInstallProtocol
    {
        public string FileName;
        public string LocalDirectory;
        public bool FetchDirectly;
        public string DirectUrl;

        public RbxInstallProtocol(string fileName, string localDirectory)
        {
            FileName = fileName;
            LocalDirectory = localDirectory;
            FetchDirectly = false;
            DirectUrl = null;
        }

        public RbxInstallProtocol(string fileName, bool newFolderWithFileName)
        {
            FileName = fileName;
            LocalDirectory = (newFolderWithFileName ? fileName : "");
            FetchDirectly = false;
            DirectUrl = null;
        }

        public RbxInstallProtocol(string fileName, string directUrl, bool fetchDirectly = true)
        {
            FileName = fileName;
            LocalDirectory = "";
            FetchDirectly = fetchDirectly;
            DirectUrl = directUrl;
        }
    }

    public partial class RobloxInstaller : Form
    {
        public string buildName;
        public string setupDir;
        public string robloxStudioBetaPath;

        public delegate void EchoDelegator(string text);
        public delegate void IncrementDelegator(int count);

        private int actualProgressBarSum = 0;
        private static string gitContentUrl = "https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Studio-Mod-Manager/master/";

        WebClient http = new WebClient();
        List<RbxInstallProtocol> instructions = new List<RbxInstallProtocol>();

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

        private void incrementProgressBarMax(int count)
        {
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new IncrementDelegator(incrementProgressBarMax), count);
            else
            {
                actualProgressBarSum += count;
                if (actualProgressBarSum > progressBar.Maximum)
                    progressBar.Maximum = actualProgressBarSum;
            }
        }

        private void incrementProgress(int count = 1)
        {
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new IncrementDelegator(progressBar.Increment), count);
            else
                progressBar.Increment(count);
        }

        private void loadInstructions(StringReader installProtocol)
        {
            string currentCmd = null;
            string line = null;
            while ((line = installProtocol.ReadLine()) != null)
            {
                if (line.Length > 0 && !line.StartsWith("--")) // No comments or empty lines
                {
                    if (line.StartsWith("/"))
                        currentCmd = line.Substring(1);
                    else
                    {
                        string[] dashSplit = line.Split('-');
                        string directoryLine = Regex.Replace(dashSplit[dashSplit.Length - 1], @"[\d-]", ""); // oof
                        if (currentCmd == "ExtractContent")
                            instructions.Add(new RbxInstallProtocol("content-" + line, @"content\" + directoryLine));
                        else if (currentCmd == "ExtractPlatformContent")
                            instructions.Add(new RbxInstallProtocol("content-" + line, @"PlatformContent\pc\" + directoryLine));
                        else if (currentCmd == "ExtractDirect")
                            instructions.Add(new RbxInstallProtocol(line, false));
                        else if (currentCmd == "ExtractAsFolder")
                            instructions.Add(new RbxInstallProtocol(line, true));
                    }
                }
            }
        }

        public RobloxInstaller()
        {
            InitializeComponent();
            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
        }

        private static string getDirectory(params string[] paths)
        {
            string basePath = "";
            foreach (string path in paths)
                basePath = Path.Combine(basePath, path);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            return basePath;
        }

        public void applyFVariableConfiguration(RegistryKey fvarRegistry, string filePath)
        {
            List<string> prefixes = new List<string>() { "F", "DF", "SF" }; // List of possible prefixes that the fvars will use.
            try
            {
                List<string> configs = new List<string>();
                foreach (string fvar in fvarRegistry.GetSubKeyNames())
                {
                    RegistryKey fvarEntry = Program.GetSubKey(fvarRegistry, fvar);
                    string type = fvarEntry.GetValue("Type") as string;
                    string value = fvarEntry.GetValue("Value") as string;
                    string key = type + fvar;
                    foreach (string prefix in prefixes)
                        configs.Add('"' + prefix + key + "\":\"" + value + '"');
                };

                string json = "{" + string.Join(",", configs.ToArray()) + "}";
                File.WriteAllText(filePath, json);
            }
            catch
            {
                echo("Failed to apply FVariable configuration!");
            }
        }

        public async Task<string> RunInstaller(string database, bool forceInstall)
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string rootDir = getDirectory(localAppData, "Roblox Studio");
            string downloads = getDirectory(rootDir, "downloads");

            RegistryKey checkSum = Program.GetSubKey(Program.ModManagerRegistry, "Checksum");
            
            Show();
            BringToFront();

            await setStatus("Checking for updates");

            List<string> checkSumKeys = new List<string>(checkSum.GetValueNames());

            if (database == "future-is-bright")
            {
                // This is an ugly hack, but it doesn't require as much special casing for the installation protocol.

                string latestRelease = await http.DownloadStringTaskAsync("https://api.github.com/repos/roblox/future-is-bright/releases/latest");
                int urlBegin = latestRelease.IndexOf("https://github.com/Roblox/future-is-bright/releases/download/");
                int urlEnd = latestRelease.IndexOf(".zip", urlBegin + 1)+4;

                string url = latestRelease.Substring(urlBegin, urlEnd - urlBegin);
                int lastSlash = url.LastIndexOf('/');
                string zipFileName = url.Substring(lastSlash + 1, url.Length - lastSlash - 5);

                buildName = zipFileName;
                setupDir = "github";

                instructions.Add(new RbxInstallProtocol(zipFileName, url, true));

                foreach (string key in checkSumKeys)
                    if (key != "future-is-bright")
                        checkSum.DeleteValue(key);
            }
            else
            {
                echo("Fetching API Key for Version Fetch...");
                string apiKey = await http.DownloadStringTaskAsync(gitContentUrl + "VersionCompatibilityApiKey");
                string versionUrl = "http://versioncompatibility.api." + database + 
                                    ".com/GetCurrentClientVersionUpload/?apiKey=" + apiKey + 
                                    "&binaryType=WindowsStudio";

                echo("Fetching Version GUID...");
                setupDir = "setup." + database + ".com/";
                buildName = await http.DownloadStringTaskAsync(versionUrl);
                buildName = buildName.Replace('"', ' ').Trim();
                if (checkSumKeys.Contains("future-is-bright"))
                    checkSum.DeleteSubKey("future-is-bright");
            }

            robloxStudioBetaPath = Path.Combine(rootDir, "RobloxStudioBeta.exe");

            echo("Checking build installation...");

            string currentBuildDatabase = Program.ModManagerRegistry.GetValue("BuildDatabase","") as string;
            string currentBuildVersion = Program.ModManagerRegistry.GetValue("BuildVersion", "") as string;

            if (currentBuildDatabase != database || currentBuildVersion != buildName || forceInstall)
            {
                echo("This build needs to be installed!");
                await setStatus("Loading the latest '" + database + "' build of Roblox Studio...");
                progressBar.Maximum = 1300; // Rough estimate of how many files to expect.
                progressBar.Value = 0;
                progressBar.Style = ProgressBarStyle.Continuous;

                bool safeToContinue = false;
                bool cancelled = false;

                while (!safeToContinue)
                {
                    Process[] running = Process.GetProcessesByName("RobloxStudioBeta");
                    if (running.Length > 0)
                    {
                        foreach (Process p in running)
                        {
                            p.CloseMainWindow();
                            Program.SetForegroundWindow(p.MainWindowHandle);
                            await Task.Delay(50);
                        }
                        await Task.Delay(1000);
                        Process[] runningNow = Process.GetProcessesByName("RobloxStudioBeta");
                        BringToFront();
                        if (runningNow.Length > 0)
                        {
                            echo("Running apps detected, action on the user's part is needed.");
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
                    if (buildName.Substring(0,16) != "future-is-bright")
                    {
                        string installerProtocol = await http.DownloadStringTaskAsync(gitContentUrl + "InstallerProtocol");
                        StringReader protocolReader = new StringReader(installerProtocol);
                        loadInstructions(protocolReader);
                    }

                    foreach (RbxInstallProtocol protocol in instructions)
                    {
                        Task installer = Task.Run(async () =>
                        {
                            string zipFileUrl;
                            if (protocol.FetchDirectly)
                                zipFileUrl = protocol.DirectUrl;
                            else
                                zipFileUrl = "https://" + setupDir + buildName + "-" + protocol.FileName + ".zip";

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
                                string fileHash = null;
                                if (!(forceInstall || database == "future-is-bright"))
                                {
                                    SHA256Managed sha = new SHA256Managed();
                                    MemoryStream fileStream = new MemoryStream(downloadedFile);
                                    BufferedStream buffered = new BufferedStream(fileStream, 1200000);
                                    byte[] hash = sha.ComputeHash(buffered);
                                    fileHash = Convert.ToBase64String(hash);
                                    string currentHash = checkSum.GetValue(protocol.FileName, "") as string;
                                    if (fileHash == currentHash)
                                    {
                                        echo(protocol.FileName + ".zip hasn't changed between builds, skipping.");
                                        doInstall = false;
                                    }
                                }
                                FileStream writeFile = File.Create(downloadPath);
                                writeFile.Write(downloadedFile, 0, downloadedFile.Length);
                                writeFile.Close();
                                ZipArchive archive = ZipFile.Open(downloadPath, ZipArchiveMode.Read);
                                incrementProgressBarMax(archive.Entries.Count);
                                if (doInstall)
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        string name = entry.Name.Replace(protocol.FileName + "/", "");
                                        string entryName = entry.FullName.Replace(protocol.FileName + "/", "");
                                        if (entryName.Length > 0)
                                        {
                                            if (string.IsNullOrEmpty(name))
                                            {
                                                string localPath = Path.Combine(protocol.LocalDirectory, entryName);
                                                string path = Path.Combine(rootDir, localPath);
                                                echo("Creating directory " + localPath);
                                                getDirectory(path);
                                            }
                                            else
                                            {
                                                echo("Extracting " + entryName + " to " + extractDir);
                                                string relativePath = Path.Combine(extractDir, entryName);
                                                string directoryParent = Directory.GetParent(relativePath).ToString();
                                                getDirectory(directoryParent);
                                                try
                                                {
                                                    if (File.Exists(relativePath))
                                                        File.Delete(relativePath);
                                                    entry.ExtractToFile(relativePath);
                                                }
                                                catch
                                                {
                                                    echo("Couldn't update " + entryName);
                                                }
                                            }
                                        }
                                        incrementProgress();
                                    }

                                    if (fileHash != null)
                                        checkSum.SetValue(protocol.FileName, fileHash);
                                }
                                else
                                {
                                    incrementProgress(archive.Entries.Count);
                                }
                                localHttp.Dispose();
                            }
                            catch
                            {
                                echo(zipFileUrl + " is currently not available. This build may not run correctly?");
                            }
                        });
                        taskQueue.Add(installer);
                        await Task.Delay(50);
                    }
                    await Task.WhenAll(taskQueue.ToArray());

                    await setStatus("Configuring Roblox Studio");
                    progressBar.Style = ProgressBarStyle.Marquee;

                    Program.ModManagerRegistry.SetValue("BuildDatabase", database);
                    Program.ModManagerRegistry.SetValue("BuildVersion", buildName);

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
            Program.UpdateStudioRegistryProtocols(setupDir, buildName, robloxStudioBetaPath);

            RegistryKey fvarRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FVariables");
            string clientSettings = getDirectory(rootDir, "ClientSettings");
            string clientAppSettings = Path.Combine(clientSettings, "ClientAppSettings.json");
            applyFVariableConfiguration(fvarRegistry, clientAppSettings);

            await setStatus("Starting Roblox Studio...");

            return robloxStudioBetaPath;
        }

        private void RobloxInstaller_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}