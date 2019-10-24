using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class StudioBootstrapper : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        public delegate void EchoDelegator(string text);
        public delegate void IncrementDelegator(int count);
        
        private string buildVersion;
        private FileManifest fileManifest;
        private bool forceInstall = false;
        private bool exitWhenClosed = true;
        private int actualProgressBarSum = 0;

        private const string appSettingsXml =
            "<Settings>\n" +
            "   <ContentFolder>content</ContentFolder>\n" +
            "   <BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>";

        private static WebClient http = new WebClient();

        private static RegistryKey versionRegistry = Program.VersionRegistry;
        private static RegistryKey pkgRegistry = Program.GetSubKey("PackageManifest");
        
        private static RegistryKey fileRegistry = Program.GetSubKey("FileManifest");
        private static RegistryKey fileRepairs = fileRegistry.GetSubKey("Repairs");
        
        public StudioBootstrapper(bool forceInstall = false, bool exitWhenClosed = true)
        {
            InitializeComponent();
            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");

            this.forceInstall = forceInstall;
            this.exitWhenClosed = exitWhenClosed;
        }

        private static string computeSignature(Stream source)
        {
            string result;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(source);
                result = BitConverter.ToString(hash)
                    .Replace("-", "")
                    .ToLower();
            }

            return result;
        }

        private static string computeSignature(ZipArchiveEntry entry)
        {
            using (Stream stream = entry.Open())
            {
                return computeSignature(stream);
            }
        }

        private void setStatus(string status)
        {
            statusLbl.Text = status;
            statusLbl.Refresh();
        }

        private void restore()
        {
            TopMost = true;
            BringToFront();
            Show();
        }

        private static bool tryToKillProcess(Process process)
        {
            try
            {
                process.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void echo(string text)
        {
            if (log.InvokeRequired)
            {
                var echoer = new EchoDelegator(echo);
                log.Invoke(echoer, text);
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
                var increment = new IncrementDelegator(incrementProgressBarMax);
                progressBar.Invoke(increment, count);
            }
            else
            {
                actualProgressBarSum += count;
                progressBar.Maximum = Math.Max(progressBar.Maximum, actualProgressBarSum);
            }
        }

        private void incrementProgress(int count = 1)
        {
            if (progressBar.InvokeRequired)
            {
                var increment = new IncrementDelegator(progressBar.Increment);
                progressBar.Invoke(increment, count);
            }
            else
            {
                progressBar.Increment(count);
            }
        }
        
        private static string getDirectory(params string[] paths)
        {
            string basePath = Path.Combine(paths);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return basePath;
        }
        
        private Task deleteUnusedFiles()
        {
            var taskQueue = new List<Task>();
            string studioDir = GetStudioDirectory();

            foreach (string rawFileName in fileRegistry.GetValueNames())
            {
                // Temporary variable so we can change what file we are testing,
                // without breaking the foreach loop.
                string fileName = rawFileName;

                // A few hacky exemptions to the rules, but necessary because older versions of the
                // mod manager do not record which files are outside of the manifest, but valid otherwise.
                // TODO: Need a more proper way of handling this

                if (fileName.Contains("/") || fileName.EndsWith(".dll") && !fileName.Contains("\\"))
                    continue;

                string filePath = Path.Combine(studioDir, fileName);

                if (!File.Exists(filePath))
                {
                    // Check if we recorded this file as an error in the manifest.
                    string fixedFile = fileRepairs.GetString(fileName);

                    if (fixedFile.Length > 0)
                    {
                        // Use this path instead.
                        filePath = Path.Combine(studioDir, fixedFile);
                        fileName = fixedFile;
                    }
                }

                if (!fileManifest.FileToSignature.ContainsKey(fileName))
                {
                    if (File.Exists(filePath))
                    {
                        Task verify = Task.Run(() =>
                        {
                            try
                            {
                                // Confirm that this file no longer exists in the manifest.
                                string signature;

                                using (FileStream file = File.OpenRead(filePath))
                                    signature = computeSignature(file);

                                if (!fileManifest.SignatureToFiles.ContainsKey(signature))
                                {
                                    echo($"Deleting unused file {fileName}");
                                    File.Delete(filePath);
                                    fileRegistry.DeleteValue(fileName);
                                }
                                else if (!fileName.StartsWith("content"))
                                {
                                    // The path may have been labeled incorrectly in the manifest.
                                    // Record it for future reference so we don't have to
                                    // waste time computing the signature later on.

                                    string fixedName = fileManifest.SignatureToFiles[signature].First();
                                    fileRepairs.SetValue(fileName, fixedName);
                                }
                            }
                            catch
                            {
                                Console.WriteLine("FAILED TO VERIFY OR DELETE " + fileName);
                            }
                        });

                        taskQueue.Add(verify);
                    }
                    else
                    {
                        fileRegistry.DeleteValue(fileName);
                    }
                }
            }

            return Task.WhenAll(taskQueue.ToArray());
        }

        private async Task<bool> shutdownStudioProcesses()
        {
            bool cancelled = false;
            bool safeToContinue = false;

            while (!safeToContinue)
            {
                List<Process> running = GetRunningStudioProcesses();

                if (running.Count > 0)
                {
                    setStatus("Shutting down Roblox Studio...");

                    TopMost = false;
                    Hide();

                    foreach (Process p in running)
                    {
                        SetForegroundWindow(p.MainWindowHandle);
                        FlashWindow(p.MainWindowHandle, true);

                        p.CloseMainWindow();
                        await Task.Delay(50);
                    }
                    
                    List<Process> runningNow = null;

                    const int retries = 10;
                    const int granularity = 300;

                    restore();

                    progressBar.Value = 0;
                    progressBar.Maximum = retries * granularity;

                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Refresh();

                    for (int i = 0; i < retries; i++)
                    {
                        runningNow = GetRunningStudioProcesses();

                        if (runningNow.Count == 0)
                        {
                            safeToContinue = true;
                            break;
                        }
                        else
                        {
                            progressBar.Increment(granularity);
                            await Task.Delay(1000);
                        }
                    }

                    if (runningNow.Count > 0 && !safeToContinue)
                    {
                        restore();

                        DialogResult result = MessageBox.Show
                        (
                            "All Roblox Studio processes need to be closed in order to update Roblox Studio!\n" +
                            "Press Ok once you've saved your work, or\n" +
                            "Press Cancel to skip this update temporarily.",

                            "Notice",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Warning
                        );

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

            return !cancelled;
        }

        public static async Task BringUpToDate(string branch, string expectedVersion, string updateReason)
        {
            string currentVersion = versionRegistry.GetString("VersionGuid");

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
                    StudioBootstrapper installer = new StudioBootstrapper(false);

                    await installer.RunInstaller(branch);
                    installer.Dispose();
                }
            }
        }

        public static string GetStudioBinaryType()
        {
            string binaryType = "WindowsStudio";

            if (Program.GetString("BuildType") == "64-bit")
                binaryType += "64";

            return binaryType;
        }

        // This does a quick check of /versionQTStudio without resolving
        // if its the proper version-guid for gametest builds. This should
        // make gametest update checks faster... at least for 64-bit users.
        public static async Task<string> GetFastVersionGuid(string branch)
        {
            if (branch == "roblox")
            {
                string binaryType = GetStudioBinaryType();
                var info = await ClientVersionInfo.Get(binaryType);

                return info.Guid;
            }
            else
            {
                string fastUrl = $"https://s3.amazonaws.com/setup.{branch}.com/versionQTStudio";
                return await http.DownloadStringTaskAsync(fastUrl);
            }
        }

        public static async Task<ClientVersionInfo> GetCurrentVersionInfo(string branch, string fastVersionGuid = "")
        {
            string binaryType = GetStudioBinaryType();
            
            if (branch == "roblox")
                return await ClientVersionInfo.Get(binaryType);

            if (fastVersionGuid != "")
            {
                string latestGuid;

                if (binaryType == "WindowsStudio64")
                    latestGuid = versionRegistry.GetString("LatestGuid_x86");
                else
                    latestGuid = versionRegistry.GetString("LatestGuid_x64");

                // If we already determined the fast version guid is pointing
                // to the other version of Roblox Studio, fallback to the
                // version data that has been cached already.

                if (fastVersionGuid == latestGuid)
                {
                    string versionId = versionRegistry.GetString("Version");
                    string versionGuid = versionRegistry.GetString("VersionGuid");

                    ClientVersionInfo proxy = new ClientVersionInfo()
                    {
                        Version = versionId,
                        Guid = versionGuid
                    };

                    return proxy;
                }
            }

            // Unfortunately, the ClientVersionInfo end-point on gametest
            // isn't available to the public, so I have to parse the 

            var logData = await StudioDeployLogs.Get(branch);

            var currentLogs = logData.CurrentLogs;
            int numLogs = currentLogs.Count;

            var latest = currentLogs[numLogs - 1];
            var prev = currentLogs[numLogs - 2];

            DeployLog build_x86, build_x64;

            if (logData.Has64BitLogs)
            {
                if (prev.Is64Bit)
                {
                    build_x64 = prev;
                    build_x86 = latest;
                }
                else
                {
                    build_x64 = latest;
                    build_x86 = prev;
                }
            }
            else
            {
                if (prev.Changelist != latest.Changelist)
                {
                    build_x86 = latest;
                    build_x64 = prev;
                }
                else
                {
                    build_x86 = prev;
                    build_x64 = latest;
                }
            }

            var info = new ClientVersionInfo();

            if (binaryType == "WindowsStudio64")
            {
                info.Version = build_x64.ToString();
                info.Guid = build_x64.VersionGuid;
            }
            else
            {
                info.Version = build_x86.ToString();
                info.Guid = build_x86.VersionGuid;
            }

            versionRegistry.SetValue("LatestGuid_x86", build_x86.VersionGuid);
            versionRegistry.SetValue("LatestGuid_x64", build_x64.VersionGuid);

            return info;
        }

        // YOU WERE SO CLOSE ROBLOX, AGHHHH
        private static string fixFilePath(string pkgName, string filePath)
        {
            string pkgDir = pkgName.Replace(".zip", "");

            if ((pkgDir == "Plugins" || pkgDir == "Qml") && !filePath.StartsWith(pkgDir))
                filePath = pkgDir + '\\' + filePath;

            return filePath;
        }

        private async Task installPackage(PackageManifest package)
        {
            string studioDir = GetStudioDirectory();
            string downloads = getDirectory(studioDir, "downloads");

            string pkgName = package.Name;
            string branch = package.Branch;

            string oldSig = pkgRegistry.GetString(pkgName);
            string newSig = package.Signature;

            if (oldSig == newSig && !forceInstall)
            {
                echo($"Package '{pkgName}' hasn't changed between builds, skipping.");
                return;
            }

            string zipFileUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{buildVersion}-{pkgName}";
            string zipExtractPath = Path.Combine(downloads, package.Name);

            incrementProgressBarMax(package.Size);
            echo($"Installing package {zipFileUrl}");

            var localHttp = new WebClient();
            localHttp.Headers.Set("UserAgent", "Roblox");

            // Download the zip file package.
            byte[] fileContents = await localHttp.DownloadDataTaskAsync(zipFileUrl);

            // If the size of the file we downloaded does not match the packed size specified
            // in the manifest, then this file has been tampered with.

            if (fileContents.Length != package.PackedSize)
                throw new InvalidDataException($"{package.Name} expected packed size: {package.PackedSize} but got: {fileContents.Length}");

            using (MemoryStream fileBuffer = new MemoryStream(fileContents))
            {
                // Compute the MD5 signature of this zip file, and make sure it matches with the
                // signature specified in the package manifest.
                string checkSig = computeSignature(fileBuffer);

                if (checkSig != newSig)
                    throw new InvalidDataException($"{package.Name} expected signature: {newSig} but got: {checkSig}");

                // Write the zip file.
                File.WriteAllBytes(zipExtractPath, fileContents);
            }

            ZipArchive archive = ZipFile.OpenRead(zipExtractPath);
            string localRootDir = null;

            var reprocess = new Dictionary<ZipArchiveEntry, string>();

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Length > 0)
                {
                    string newFileSig = null;

                    // If we have figured out what our root directory is, try to resolve
                    // what the signature of this file is.

                    if (localRootDir != null)
                    {
                        var fileSignatures = fileManifest.FileToSignature;

                        string filePath = entry.FullName.Replace('/', '\\');
                        bool hasFilePath = fileSignatures.ContainsKey(filePath);

                        // If we can't find this file in the signature lookup table,
                        // try appending the local directory to it. This resolves some
                        // edge cases relating to the fixFilePath function above.

                        if (!hasFilePath)
                        {
                            filePath = localRootDir + filePath;
                            hasFilePath = fileSignatures.ContainsKey(filePath);
                        }

                        // If we can find this file path in the file manifest, then we will
                        // use its pre-computed signature to check if the file has changed.

                        newFileSig = hasFilePath ? fileSignatures[filePath] : null;
                    }

                    // If we couldn't pre-determine the file signature from the manifest,
                    // then we have to compute it manually. This is slower.

                    if (newFileSig == null)
                        newFileSig = computeSignature(entry);

                    // Now check what files this signature corresponds with.
                    if (fileManifest.SignatureToFiles.ContainsKey(newFileSig))
                    {
                        // Write this package to each of the files specified.
                        List<string> files = fileManifest.SignatureToFiles[newFileSig];

                        foreach (string file in files)
                        {
                            // Write the file from this signature.
                            writePackageFile(studioDir, pkgName, file, newFileSig, entry);

                            if (localRootDir == null)
                            {
                                string filePath = fixFilePath(pkgName, file);
                                string entryPath = entry.FullName.Replace('/', '\\');
                                
                                if (filePath.EndsWith(entryPath))
                                {
                                    // We can infer what the root extraction  
                                    // directory is for the files in this package!                                 
                                    localRootDir = filePath.Replace(entryPath, "");
                                }
                            }
                        }
                    }
                    else
                    {
                        string file = entry.FullName;

                        if (localRootDir == null)
                        {
                            // Check back on this file after we extract the regular files,
                            // so we can make sure this is extracted to the correct directory.
                            reprocess.Add(entry, newFileSig);
                        }
                        else
                        {
                            // Append the local root directory.
                            file = localRootDir + file;
                            writePackageFile(studioDir, pkgName, file, newFileSig, entry);
                        }
                    }
                }
            }

            // Process any files that we deferred from writing immediately.
            foreach (ZipArchiveEntry entry in reprocess.Keys)
            {
                string file = entry.FullName;
                string newFileSig = reprocess[entry];

                if (localRootDir != null)
                    file = localRootDir + file;

                writePackageFile(studioDir, pkgName, file, newFileSig, entry);
            }

            // Update the signature in the package registry so we can check
            // if this zip file needs to be updated in future versions.
            pkgRegistry.SetValue(pkgName, package.Signature);
        }

        private void writePackageFile(string studioDir, string pkgName, string file, string newFileSig, ZipArchiveEntry entry)
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

            string extractPath = Path.Combine(studioDir, filePath);
            string extractDir = Path.GetDirectoryName(extractPath);

            getDirectory(extractDir);
            
            try
            {
                if (File.Exists(extractPath))
                    File.Delete(extractPath);

                echo($"Writing {filePath}...");
                entry.ExtractToFile(extractPath);

                fileRegistry.SetValue(filePath, newFileSig);
            }
            catch
            {
                echo($"FILE WRITE FAILED: {filePath} (This build may not run as expected!)");
            }

            incrementProgress(length);
        }

        public static string GetStudioDirectory()
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            return getDirectory(localAppData, "Roblox Studio");
        }

        public static string GetStudioPath()
        {
            string studioDir = GetStudioDirectory();
            return Path.Combine(studioDir, "RobloxStudioBeta.exe");
        }
        
        public static List<Process> GetRunningStudioProcesses()
        {
            List<Process> studioProcs = new List<Process>();
            
            foreach (Process process in Process.GetProcessesByName("RobloxStudioBeta"))
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    studioProcs.Add(process);
                }
                else
                {
                    tryToKillProcess(process);
                }
            }

            return studioProcs;
        }
        
        public async Task RunInstaller(string branch)
        {
            restore();

            setStatus("Checking for updates");
            echo("Checking build installation...");

            string currentBranch = Program.GetString("BuildBranch", "roblox");
            string currentVersion = versionRegistry.GetString("VersionGuid");

            bool shouldInstall = (forceInstall || currentBranch != branch);
            string fastVersion = await GetFastVersionGuid(currentBranch);
            
            if (branch == "roblox")
                buildVersion = fastVersion;

            ClientVersionInfo versionInfo = null;

            if (shouldInstall || fastVersion != currentVersion)
            {
                if (currentBranch != "roblox")
                    echo("Possible update detected, verifying...");

                versionInfo = await GetCurrentVersionInfo(branch, fastVersion);
                buildVersion = versionInfo.Guid;
            }
            else
            {
                buildVersion = fastVersion;
            }

            if (currentVersion != buildVersion || shouldInstall)
            {
                echo("This build needs to be installed!");
                bool studioClosed = await shutdownStudioProcesses();

                if (studioClosed)
                {
                    string binaryType = GetStudioBinaryType();
                    string studioDir = GetStudioDirectory();
                    string versionId = versionInfo.Version;

                    restore();
                    setStatus($"Installing Version {versionId} of Roblox Studio...");

                    var taskQueue = new List<Task>();

                    echo("Grabbing package manifest...");
                    List<PackageManifest> pkgManifest = await PackageManifest.Get(branch, buildVersion);

                    echo("Grabbing file manifest...");
                    fileManifest = await FileManifest.Get(branch, buildVersion);

                    progressBar.Maximum = 1;
                    progressBar.Value = 0;

                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Refresh();
                    
                    foreach (PackageManifest package in pkgManifest)
                    {
                        Task installer = Task.Run(() => installPackage(package));
                        taskQueue.Add(installer);
                    }

                    await Task.WhenAll(taskQueue.ToArray());

                    echo("Writing AppSettings.xml...");

                    string appSettings = Path.Combine(studioDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, appSettingsXml);

                    setStatus("Deleting unused files...");
                    await deleteUnusedFiles();

                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.Refresh();

                    Program.SetValue("BuildBranch", branch);

                    versionRegistry.SetValue("Version", versionId);
                    versionRegistry.SetValue("VersionGuid", buildVersion);
                }
                else
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.Refresh();

                    echo("Update cancelled. Launching on current branch and version.");
                }
            }
            else
            {
                echo("This version of Roblox Studio has been installed!");
            }
            
            setStatus("Configuring Roblox Studio...");

            echo("Updating registry protocols...");
            Program.UpdateStudioRegistryProtocols();

            if (exitWhenClosed)
            {
                echo("Applying flag configuration...");
                FlagEditor.ApplyFlags();

                echo("Patching explorer icons...");
                await ClassIconEditor.PatchExplorerIcons();
            }
            
            setStatus("Starting Roblox Studio...");
            echo("Roblox Studio is up to date!");

            await Task.Delay(1000);
        }

        private void StudioBootstrapper_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exitWhenClosed)
            {
                Application.Exit();
            }
        }
    }
}
