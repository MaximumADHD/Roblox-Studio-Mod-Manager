using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public struct RobloxPackageManifest
    {
        public string Name;
        public string Signature;
        public int PackedSize;
        public int Size;

        public override string ToString()
        {
            return Name + " = " + Signature + " [Packed-Size: " + PackedSize + "][Size: " + Size + ']';
        }
    }

    public struct RobloxFileManifest
    {
        public Dictionary<string, List<string>> SignatureToFiles;
        public Dictionary<string, string> FileToSignature;
    }

    public partial class RobloxInstaller : Form
    {
        public string buildVersion;
        public string setupDir;
        public string robloxStudioBetaPath;

        public delegate void EchoDelegator(string text);
        public delegate void IncrementDelegator(int count);

        private int actualProgressBarSum = 0;
        private bool exitWhenClosed = true;

        private static string gitContentUrl = "https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Studio-Mod-Manager/master/";
        private static string amazonAws = "https://s3.amazonaws.com/";

        private static WebClient http = new WebClient();
        private static string versionCompKey;

        private static RegistryKey pkgRegistry = Program.GetSubKey(Program.ModManagerRegistry, "PackageManifest");
        private static RegistryKey fileRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FileManifest");

        private static string computeSignature(Stream source)
        {
            string result;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(source);
                result = BitConverter.ToString(hash);
                result = result.Replace("-", "");
                result = result.ToLower();
            }

            return result;
        }

        private void setStatus(string status)
        {
            statusLbl.Text = status;
            statusLbl.Refresh();
        }

        private void echo(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new EchoDelegator(echo), text);
            }
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
            {
                progressBar.Invoke(new IncrementDelegator(incrementProgressBarMax), count);
            }
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

        private string constructDownloadUrl(string file)
        {
            return amazonAws + setupDir + buildVersion + '-' + file;
        }

        private async Task<List<RobloxPackageManifest>> getPackageManifest()
        {
            string pkgManifestUrl = constructDownloadUrl("rbxPkgManifest.txt");
            string pkgManifestData = await http.DownloadStringTaskAsync(pkgManifestUrl);

            List<RobloxPackageManifest> result = new List<RobloxPackageManifest>();

            using (StringReader reader = new StringReader(pkgManifestData))
            {
                string version = reader.ReadLine();
                if (version != "v0")
                    throw new NotSupportedException("Unexpected package manifest version: " + version + " (expected v0!)\nPlease contact CloneTrooper1019 if you see this error.");

                while (true)
                {
                    try
                    {
                        string fileName = reader.ReadLine();
                        string signature = reader.ReadLine();
                        int packedSize = int.Parse(reader.ReadLine());
                        int size = int.Parse(reader.ReadLine());

                        if (fileName.EndsWith(".zip"))
                        {
                            RobloxPackageManifest pkgManifest = new RobloxPackageManifest();
                            pkgManifest.Name = fileName;
                            pkgManifest.Signature = signature;
                            pkgManifest.PackedSize = packedSize;
                            pkgManifest.Size = size;

                            result.Add(pkgManifest);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private async Task<RobloxFileManifest> getFileManifest()
        {
            string fileManifestUrl = constructDownloadUrl("rbxManifest.txt");
            string fileManifestData = await http.DownloadStringTaskAsync(fileManifestUrl);

            RobloxFileManifest result = new RobloxFileManifest();
            result.FileToSignature = new Dictionary<string, string>();
            result.SignatureToFiles = new Dictionary<string, List<string>>();

            using (StringReader reader = new StringReader(fileManifestData))
            {
                string path = "";
                string signature = "";

                while (path != null && signature != null)
                {
                    try
                    {
                        path = reader.ReadLine();
                        signature = reader.ReadLine();

                        if (path == null || signature == null)
                            break;

                        if (!result.SignatureToFiles.ContainsKey(signature))
                            result.SignatureToFiles.Add(signature, new List<string>());

                        result.SignatureToFiles[signature].Add(path);
                        result.FileToSignature.Add(path, signature);
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public static async Task BringUpToDate(string branch, string expectedVersion, string updateReason)
        {
            string currentVersion = Program.GetRegistryString(Program.ModManagerRegistry, "BuildVersion");

            if (currentVersion != expectedVersion)
            {
                DialogResult check = MessageBox.Show
                (
                    "Roblox Studio is out of date!\n" 
                    + updateReason + 
                    "\nWould you like to update now?",
                    
                    "Out of date!",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (check == DialogResult.Yes)
                {
                    RobloxInstaller installer = new RobloxInstaller(false);

                    await installer.RunInstaller(branch);
                    installer.Dispose();
                }
            }
        }

        public RobloxInstaller(bool _exitWhenClosed = true)
        {
            InitializeComponent();
            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
            exitWhenClosed = _exitWhenClosed;
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

        public void applyFlagEditorConfiguration(RegistryKey flagRegistry, string filePath)
        {
            try
            {
                List<string> configs = new List<string>();

                foreach (string flagName in flagRegistry.GetSubKeyNames())
                {
                    RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                    string name  = flagKey.GetValue("Name")  as string;
                    string type  = flagKey.GetValue("Type")  as string;
                    string value = flagKey.GetValue("Value") as string;

                    string key = type + name;
                    if (type.EndsWith("String"))
                        value = '"' + value.Replace("\"", "\\\"").Replace("\\\\","\\") + '"';

                    configs.Add("\t\"" + key + "\": " + value);
                };

                string json = "{\r\n" + string.Join(",\r\n", configs.ToArray()) + "\r\n}";
                File.WriteAllText(filePath, json);
            }
            catch
            {
                echo("Failed to apply FVariable configuration!");
            }
        }

        public static async Task<string> GetCurrentVersion(string branch)
        {
            if (versionCompKey == null)
                versionCompKey = await http.DownloadStringTaskAsync(gitContentUrl + "VersionCompatibilityApiKey");

            string versionUrl = "https://versioncompatibility.api." + branch +
                                ".com/GetCurrentClientVersionUpload/?apiKey=" + versionCompKey +
                                "&binaryType=WindowsStudio";

            string buildVersion = await http.DownloadStringTaskAsync(versionUrl);
            buildVersion = buildVersion.Replace('"', ' ').Trim();

            return buildVersion;
        }

        // YOU WERE SO CLOSE ROBLOX, AGHHHH
        private static string fixFilePath(string pkgName, string filePath)
        {
            if (pkgName == "Plugins.zip" || pkgName == "Qml.zip")
            {
                string rootPkgDir = pkgName.Replace(".zip", "");

                if (!filePath.StartsWith(rootPkgDir))
                    filePath = rootPkgDir + '\\' + filePath;

            }

            return filePath;
        }

        private void writePackageFile(string rootDir, string pkgName, string file, string newFileSig, ZipArchiveEntry entry, bool forceInstall = false)
        {
            string filePath = fixFilePath(pkgName, file);
            int length = (int)entry.Length;

            if (!forceInstall)
            {
                string oldFileSig = fileRegistry.GetValue(filePath, "") as string;
                if (oldFileSig == newFileSig)
                {
                    incrementProgress(length);
                    return;
                }
            }

            string extractPath = Path.Combine(rootDir, filePath);
            string extractDir = Path.GetDirectoryName(extractPath);

            if (!Directory.Exists(extractDir))
                Directory.CreateDirectory(extractDir);

            try
            {
                if (File.Exists(extractPath))
                    File.Delete(extractPath);

                echo("Writing " + filePath + "...");
                entry.ExtractToFile(extractPath);

                fileRegistry.SetValue(filePath, newFileSig);
            }
            catch
            {
                echo("FILE WRITE FAILED: " + filePath + " (This build may not run as expected)");
            }

            incrementProgress(length);
        }

        public static string GetStudioDirectory()
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            return getDirectory(localAppData, "Roblox Studio");
        }

        public async Task<string> RunInstaller(string branch, bool forceInstall = false)
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string rootDir = getDirectory(localAppData, "Roblox Studio");
            string downloads = getDirectory(rootDir, "downloads");

            Show();
            BringToFront();

            if (!exitWhenClosed)
            {
                FormBorderStyle = FormBorderStyle.None;
                TopMost = true;
            }

            setStatus("Checking for updates");

            setupDir = "setup." + branch + ".com/";
            buildVersion = await GetCurrentVersion(branch);
            robloxStudioBetaPath = Path.Combine(rootDir, "RobloxStudioBeta.exe");

            echo("Checking build installation...");

            string currentBranch = Program.ModManagerRegistry.GetValue("BuildBranch","") as string;
            string currentVersion = Program.ModManagerRegistry.GetValue("BuildVersion", "") as string;

            if (currentBranch != branch || currentVersion != buildVersion || forceInstall)
            {
                echo("This build needs to be installed!");

                setStatus("Installing the latest '" + (branch == "roblox" ? "production" : branch) + "' branch of Roblox Studio...");

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
                    {
                        safeToContinue = true;
                    }
                }

                if (!cancelled)
                {
                    List<Task> taskQueue = new List<Task>();

                    echo("Grabbing package manifest...");
                    List<RobloxPackageManifest> pkgManifest = await getPackageManifest();

                    echo("Grabbing file manifest...");
                    RobloxFileManifest fileManifest = await getFileManifest();

                    progressBar.Maximum = 1;
                    progressBar.Value = 0;
                    progressBar.Style = ProgressBarStyle.Continuous;

                    foreach (RobloxPackageManifest package in pkgManifest)
                    {
                        int size = package.Size;

                        string pkgName = package.Name;
                        string oldSig = pkgRegistry.GetValue(pkgName, "") as string;
                        string newSig = package.Signature;

                        if (oldSig == newSig && !forceInstall)
                        {
                            echo("Package '" + pkgName + "' hasn't changed between builds, skipping.");
                            continue;
                        }

                        progressBar.Maximum += size;

                        Task installer = Task.Run( async() =>
                        {
                            string zipFileUrl = constructDownloadUrl(package.Name);
                            string zipExtractPath = Path.Combine(downloads, package.Name);

                            echo("Installing package " + zipFileUrl);

                            try
                            {
                                WebClient localHttp = new WebClient();
                                localHttp.Headers.Set("UserAgent", "Roblox");

                                byte[] fileContents = await localHttp.DownloadDataTaskAsync(zipFileUrl);
                                if (fileContents.Length != package.PackedSize)
                                    throw new InvalidDataException(package.Name + " expected packed size: " + package.PackedSize + " but got: " + fileContents.Length);

                                using (MemoryStream fileBuffer = new MemoryStream(fileContents))
                                {
                                    string checkSig = computeSignature(fileBuffer);
                                    if (checkSig != newSig)
                                        throw new InvalidDataException(package.Name + " expected signature: " + newSig + " but got: " + checkSig);
                                }

                                File.WriteAllBytes(zipExtractPath, fileContents);
                                ZipArchive archive = ZipFile.OpenRead(zipExtractPath);

                                string localRootDir = null;

                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    if (entry.Length > 0)
                                    {
                                        string newFileSig = null;

                                        if (localRootDir != null)
                                        {
                                            string filePath = entry.FullName.Replace('/', '\\');
                                            if (!fileManifest.FileToSignature.ContainsKey(filePath))
                                                filePath = localRootDir + filePath;

                                            if (fileManifest.FileToSignature.ContainsKey(filePath))
                                                newFileSig = fileManifest.FileToSignature[filePath];

                                        }

                                        if (newFileSig == null)
                                        {
                                            using (Stream data = entry.Open())
                                                newFileSig = computeSignature(data);
                                        }

                                        if (fileManifest.SignatureToFiles.ContainsKey(newFileSig))
                                        {
                                            List<string> files = fileManifest.SignatureToFiles[newFileSig];
                                            foreach (string file in files)
                                            {
                                                writePackageFile(rootDir, pkgName, file, newFileSig, entry, forceInstall);

                                                if (localRootDir == null)
                                                {
                                                    string filePath = fixFilePath(pkgName, file);
                                                    string entryPath = entry.FullName.Replace('/', '\\');

                                                    if (filePath.EndsWith(entryPath))
                                                        localRootDir = filePath.Replace(entryPath, "");

                                                }
                                            }
                                        }
                                        else
                                        {
                                            string file = entry.FullName;
                                            writePackageFile(rootDir, pkgName, file, newFileSig, entry, forceInstall);
                                        }
                                    }
                                }

                                pkgRegistry.SetValue(pkgName, package.Signature);
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                        });

                        taskQueue.Add(installer);
                    }

                    await Task.WhenAll(taskQueue.ToArray());

                    setStatus("Writing AppSettings.xml");
                    progressBar.Style = ProgressBarStyle.Marquee;

                    Program.ModManagerRegistry.SetValue("BuildBranch", branch);
                    Program.ModManagerRegistry.SetValue("BuildVersion", buildVersion);

                    echo("Writing AppSettings.xml...");

                    string appSettings = Path.Combine(rootDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, "<Settings>\r\n\t<ContentFolder>content</ContentFolder>\r\n\t<BaseUrl>http://www.roblox.com</BaseUrl>\r\n</Settings>");
                }
                else
                {
                    echo("Update cancelled. Launching on current branch and version.");
                }
            }
            else
            {
                echo("This version of Roblox Studio has been installed!");
            }
            
            setStatus("Configuring Roblox Studio...");
            Program.UpdateStudioRegistryProtocols(setupDir, buildVersion, robloxStudioBetaPath);

            string clientSettings = getDirectory(rootDir, "ClientSettings");
            string clientAppSettings = Path.Combine(clientSettings, "ClientAppSettings.json");

            RegistryKey flagRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FlagEditor");
            applyFlagEditorConfiguration(flagRegistry, clientAppSettings);

            setStatus("Starting Roblox Studio...");
            await Task.Delay(1000);

            return robloxStudioBetaPath;
        }

        private void RobloxInstaller_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exitWhenClosed)
                Application.Exit();
        }
    }
}