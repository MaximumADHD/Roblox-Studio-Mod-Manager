using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA1031  // Do not catch general exception types
#pragma warning disable CA1303  // Do not pass literals as localized parameters
#pragma warning disable CA5351  // Do Not Use Broken Cryptographic Algorithms [ WILL FIX LATER :( ]

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
        private readonly bool forceInstall = false;
        private readonly bool exitWhenClosed = true;

        private int actualProgress = 0;
        private HashSet<string> writtenFiles;

        public SystemEvent StartEvent { get; private set; }
        
        private const string appSettingsXml =
            "<Settings>\n" +
            "   <ContentFolder>content</ContentFolder>\n" +
            "   <BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>";

        private static readonly WebClient http = new WebClient();

        private static readonly RegistryKey versionRegistry = Program.VersionRegistry;
        private static readonly RegistryKey pkgRegistry = Program.GetSubKey("PackageManifest");
        
        private static readonly RegistryKey fileRegistry = Program.GetSubKey("FileManifest");
        private static readonly RegistryKey fileRepairs = fileRegistry.GetSubKey("Repairs");
        
        public StudioBootstrapper(bool forceInstall = false, bool exitWhenClosed = true)
        {
            InitializeComponent();
            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");

            this.forceInstall = forceInstall;
            this.exitWhenClosed = exitWhenClosed;
        }

        private static string computeSignature(Stream source)
        {
            string stringHash;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(source);
                stringHash = BitConverter.ToString(hash);
            }

            string result = stringHash
                .Replace("-", "")
                .ToLower(Program.Format);

            return result;
        }

        private static string computeSignature(ZipArchiveEntry entry)
        {
            using (Stream stream = entry.Open())
            return computeSignature(stream);
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

        private static void tryToKillProcess(Process process)
        {
            try
            {
                process.Kill();
            }

            catch
            {
                Console.WriteLine($"Cannot terminate process {process.Id}!");
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
                if (log.Text.Length != 0)
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
                actualProgress += count;
                progressBar.Maximum = Math.Max(progressBar.Maximum, actualProgress);
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

            var fileNames = fileRegistry
                .GetValueNames()
                .ToList();

            foreach (string rawFileName in fileNames)
            {
                // Temporary variable so we can change what file we are testing,
                // without breaking the foreach loop.
                string fileName = rawFileName;

                // A few hacky exemptions to the rules, but necessary because older versions of the
                // mod manager do not record which files are outside of the manifest, but valid otherwise.
                // TODO: Need a more proper way of handling this

                if (fileName.Contains("/") || !fileName.Contains("\\"))
                    continue;
                
                if (fileName.EndsWith(".dll", Program.StringFormat))
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

                if (!fileManifest.ContainsKey(fileName))
                {
                    if (File.Exists(filePath))
                    {
                        Task verify = Task.Run(() =>
                        {
                            try
                            {
                                // Confirm that this file no longer exists in the manifest.
                                string sig;

                                using (FileStream file = File.OpenRead(filePath))
                                    sig = computeSignature(file);
                                
                                if (!fileManifest.ContainsValue(sig))
                                {
                                    echo($"Deleting unused file {fileName}");
                                    fileRegistry.DeleteValue(fileName);
                                    File.Delete(filePath);
                                }
                                else if (!fileName.StartsWith("content", Program.StringFormat))
                                {
                                    // The path may have been labeled incorrectly in the manifest.
                                    // Record it for future reference so we don't have to
                                    // waste time computing the signature later on.

                                    string fixedName = fileManifest
                                        .Where(pair => pair.Value == sig)
                                        .Select(pair => pair.Key)
                                        .First();

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
                    else if (fileNames.Contains(fileName))
                    {
                        fileRegistry.DeleteValue(fileName);
                    }
                }
            }

            return Task.WhenAll(taskQueue);
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

                        var delay = Task.Delay(50);
                        p.CloseMainWindow();

                        await delay.ConfigureAwait(true);
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
                            var delay = Task.Delay(1000);
                            progressBar.Increment(granularity);

                            await delay.ConfigureAwait(true);
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
                    using (var installer = new StudioBootstrapper(false))
                    {
                        var install = installer.RunInstaller(branch, false);
                        await install.ConfigureAwait(true);
                    }
                }
            }
        }

        public static string GetStudioBinaryType()
        {
            string binaryType = "WindowsStudio";

            if (Environment.Is64BitOperatingSystem)
                binaryType += "64";

            return binaryType;
        }

        // This does a quick check of /versionQTStudio without resolving
        // if its the proper version-guid for gametest builds. This should
        // make sitetest update checks faster... at least for 64-bit users.
        public static async Task<string> GetFastVersionGuid(string branch)
        {
            if (branch == "roblox")
            {
                string binaryType = GetStudioBinaryType();

                var info = await ClientVersionInfo
                    .Get(binaryType)
                    .ConfigureAwait(false);

                return info.Guid;
            }
            else
            {
                string fastUrl = $"https://s3.amazonaws.com/setup.{branch}.com/versionQTStudio";

                var result = await http
                    .DownloadStringTaskAsync(fastUrl)
                    .ConfigureAwait(false);

                return result;
            }
        }

        public static async Task<ClientVersionInfo> GetTargetVersionInfo(string branch, string targetVersion, string fastGuid = "")
        {
            var logData = await StudioDeployLogs
                .Get(branch)
                .ConfigureAwait(false);

            HashSet<DeployLog> targets;

            if (Environment.Is64BitOperatingSystem)
                targets = logData.CurrentLogs_x64;
            else
                targets = logData.CurrentLogs_x86;

            DeployLog target = targets
                .Where(log => log.VersionId == targetVersion)
                .FirstOrDefault();

            if (target == null)
            {
                Program.SetValue("TargetVersion", "");

                var result = GetCurrentVersionInfo(branch, fastGuid);
                return await result.ConfigureAwait(false);
            }

            return new ClientVersionInfo()
            {
                Guid = target.VersionGuid,
                Version = target.VersionId
            };
        }

        public static async Task<ClientVersionInfo> GetCurrentVersionInfo(string branch, string fastGuid = "")
        {
            Contract.Requires(fastGuid != null);
            string targetVersion = Program.GetString("TargetVersion");

            if (targetVersion.Length > 0)
            {
                var result = GetTargetVersionInfo(branch, targetVersion);
                return await result.ConfigureAwait(false);
            }
            
            string binaryType = GetStudioBinaryType();
            bool is64Bit = Environment.Is64BitOperatingSystem;
            
            if (branch == "roblox")
            {
                var result = ClientVersionInfo.Get(binaryType);
                return await result.ConfigureAwait(false);
            }

            if (fastGuid.Length == 0)
            {
                var getFastGuid = GetFastVersionGuid(branch);
                fastGuid = await getFastGuid.ConfigureAwait(false);
            }
            
            string latestFastGuid = versionRegistry.GetString("LatestFastGuid");
            var info = new ClientVersionInfo();

            if (latestFastGuid == fastGuid)
            {
                string version = versionRegistry.GetString("Version");
                info.Version = version;

                string latest_x86 = versionRegistry.GetString("LatestGuid_x86");
                string latest_x64 = versionRegistry.GetString("LatestGuid_x64");

                if (latestFastGuid == latest_x64 && is64Bit)
                {
                    info.Guid = latest_x64;
                    return info;
                }

                if (latestFastGuid == latest_x86 && !is64Bit)
                {
                    info.Guid = latest_x86;
                    return info;
                }
            }

            // Unfortunately the ClientVersionInfo end-point on sitetest
            // isn't available to the public, so I have to parse the
            // DeployHistory.txt file on their setup s3 bucket.

            var logData = await StudioDeployLogs
                .Get(branch)
                .ConfigureAwait(false);

            DeployLog build_x86 = logData.CurrentLogs_x86.Last();
            DeployLog build_x64 = logData.CurrentLogs_x64.Last();

            if (is64Bit)
            {
                info.Version = build_x64.VersionId;
                info.Guid = build_x64.VersionGuid;
            }
            else
            {
                info.Version = build_x86.VersionId;
                info.Guid = build_x86.VersionGuid;
            }

            versionRegistry.SetValue("LatestFastGuid", fastGuid);
            versionRegistry.SetValue("LatestGuid_x86", build_x86.VersionGuid);
            versionRegistry.SetValue("LatestGuid_x64", build_x64.VersionGuid);

            return info;
        }

        private static string fixFilePath(string pkgName, string filePath)
        {
            string pkgDir = pkgName.Replace(".zip", "");

            if ((pkgDir == "Plugins" || pkgDir == "Qml") && !filePath.StartsWith(pkgDir, Program.StringFormat))
                filePath = pkgDir + '\\' + filePath;
            
            return filePath;
        }

        private async Task installPackage(string branch, Package package)
        {
            string pkgName = package.Name;
            var pkgInfo = pkgRegistry.GetSubKey(pkgName);

            string studioDir = GetStudioDirectory();
            string downloads = getDirectory(studioDir, "downloads");

            string oldSig = pkgInfo.GetString("Signature");
            string newSig = package.Signature;

            if (oldSig == newSig && !forceInstall)
            {
                int fileCount = pkgInfo.GetInt("NumFiles");
                echo($"Package '{pkgName}' hasn't changed between builds, skipping.");

                incrementProgressBarMax(fileCount);
                incrementProgress(fileCount);

                return;
            }

            string zipFileUrl = $"https://s3.amazonaws.com/setup.{branch}.com/{buildVersion}-{pkgName}";
            string zipExtractPath = Path.Combine(downloads, package.Name);

            echo($"Installing package {zipFileUrl}");

            using (var localHttp = new WebClient())
            {
                localHttp.Headers.Set("UserAgent", "Roblox/WinInet");

                // Download the zip file package.
                byte[] fileContents = await localHttp
                    .DownloadDataTaskAsync(zipFileUrl)
                    .ConfigureAwait(false);

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
            }


            using (var archive = ZipFile.OpenRead(zipExtractPath))
            {
                var deferred = new Dictionary<ZipArchiveEntry, string>();

                int numFiles = archive.Entries
                    .Select(entry => entry.FullName)
                    .Where(name => !name.EndsWith("/", Program.StringFormat))
                    .Count();

                string localRootDir = null;
                incrementProgressBarMax(numFiles);

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Length > 0)
                    {
                        string newFileSig = null;

                        // If we have figured out what our root directory is, try to resolve
                        // what the signature of this file is.

                        if (localRootDir != null)
                        {
                            string filePath = entry.FullName.Replace('/', '\\');
                            bool hasFilePath = fileManifest.ContainsKey(filePath);

                            // If we can't find this file in the signature lookup table,
                            // try appending the local directory to it. This resolves some
                            // edge cases relating to the fixFilePath function above.

                            if (!hasFilePath)
                            {
                                filePath = localRootDir + filePath;
                                hasFilePath = fileManifest.ContainsKey(filePath);
                            }

                            // If we can find this file path in the file manifest, then we will
                            // use its pre-computed signature to check if the file has changed.

                            newFileSig = hasFilePath ? fileManifest[filePath] : null;
                        }

                        // If we couldn't pre-determine the file signature from the manifest,
                        // then we have to compute it manually. This is slower.

                        if (newFileSig == null)
                            newFileSig = computeSignature(entry);

                        // Now check what files this signature corresponds with.
                        var files = fileManifest
                            .Where(pair => pair.Value == newFileSig)
                            .Select(pair => pair.Key);

                        if (files.Any())
                        {
                            foreach (string file in files)
                            {
                                // Write the file from this signature.
                                WritePackageFile(studioDir, pkgName, file, newFileSig, entry);

                                if (localRootDir == null)
                                {
                                    string filePath = fixFilePath(pkgName, file);
                                    string entryPath = entry.FullName.Replace('/', '\\');

                                    if (filePath.EndsWith(entryPath, Program.StringFormat))
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
                                deferred.Add(entry, newFileSig);
                            }
                            else
                            {
                                // Append the local root directory.
                                file = localRootDir + file;
                                WritePackageFile(studioDir, pkgName, file, newFileSig, entry);
                            }
                        }
                    }
                }

                // Process any files that we deferred from writing immediately.
                foreach (ZipArchiveEntry entry in deferred.Keys)
                {
                    string file = entry.FullName;
                    string newFileSig = deferred[entry];

                    if (localRootDir != null)
                        file = localRootDir + file;

                    WritePackageFile(studioDir, pkgName, file, newFileSig, entry);
                }

                // Update the signature in the package registry so we can check
                // if this zip file needs to be updated in future versions.

                pkgInfo.SetValue("Signature", package.Signature);
                pkgInfo.SetValue("NumFiles", numFiles);
            }
        }

        private void WritePackageFile(string studioDir, string pkgName, string file, string newFileSig, ZipArchiveEntry entry)
        {
            string filePath = fixFilePath(pkgName, file);
            
            if (writtenFiles.Contains(filePath))
                return;

            if (!forceInstall)
            {
                string oldFileSig = fileRegistry.GetString(filePath);

                if (oldFileSig == newFileSig)
                {
                    incrementProgress();
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

            incrementProgress();
            writtenFiles.Add(filePath);
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
            var studioProcs = new List<Process>();
            
            foreach (Process process in Process.GetProcessesByName("RobloxStudioBeta"))
            {
                Action<Process> action;

                if (process.MainWindowHandle != IntPtr.Zero)
                    action = studioProcs.Add;
                else
                    action = tryToKillProcess;

                action(process);
            }

            return studioProcs;
        }
        
        public async Task RunInstaller(string branch, bool setStartEvent = false)
        {
            restore();

            setStatus("Checking for updates");
            echo("Checking build installation...");

            string currentBranch = Program.GetString("BuildBranch", "roblox");
            string currentVersion = versionRegistry.GetString("VersionGuid");

            bool shouldInstall = (forceInstall || currentBranch != branch);

            var getFastVersion = GetFastVersionGuid(currentBranch);
            string fastVersion = await getFastVersion.ConfigureAwait(true);

            ClientVersionInfo versionInfo = null;

            if (shouldInstall || fastVersion != currentVersion)
            {
                if (currentBranch != "roblox")
                    echo("Possible update detected, verifying...");

                var getVersionInfo = GetCurrentVersionInfo(branch, fastVersion);
                versionInfo = await getVersionInfo.ConfigureAwait(true);

                buildVersion = versionInfo.Guid;
            }
            else
            {
                buildVersion = fastVersion;
            }

            if (currentVersion != buildVersion || shouldInstall)
            {
                echo("This build needs to be installed!");

                var closeStudio = shutdownStudioProcesses();
                bool studioClosed = await closeStudio.ConfigureAwait(true);

                if (studioClosed)
                {
                    string binaryType = GetStudioBinaryType();
                    string studioDir = GetStudioDirectory();
                    string versionId = versionInfo.Version;

                    restore();
                    setStatus($"Installing Version {versionId} of Roblox Studio...");

                    var taskQueue = new List<Task>();
                    writtenFiles = new HashSet<string>();

                    echo("Grabbing package manifest...");

                    var pkgManifest = await PackageManifest
                        .Get(branch, buildVersion)
                        .ConfigureAwait(true);

                    echo("Grabbing file manifest...");

                    fileManifest = await FileManifest
                        .Get(branch, buildVersion)
                        .ConfigureAwait(true);

                    progressBar.Maximum = 5000;
                    progressBar.Value = 0;

                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Refresh();
                    
                    foreach (var package in pkgManifest)
                    {
                        Task installer = Task.Run(() => installPackage(branch, package));
                        taskQueue.Add(installer);
                    }

                    await Task
                        .WhenAll(taskQueue)
                        .ConfigureAwait(true);

                    echo("Writing AppSettings.xml...");

                    string appSettings = Path.Combine(studioDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, appSettingsXml);

                    setStatus("Deleting unused files...");

                    var delete = deleteUnusedFiles();
                    await delete.ConfigureAwait(true);

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
                var patch = ClassIconEditor.PatchExplorerIcons();

                await patch.ConfigureAwait(true);
            }
            
            setStatus("Starting Roblox Studio...");
            echo("Roblox Studio is up to date!");

            if (setStartEvent)
            {
                SystemEvent start = new SystemEvent("RobloxStudioModManagerStart");

                var autoExitTask = Task.Run(async () =>
                {
                    bool started = await start
                        .WaitForEvent()
                        .ConfigureAwait(true);

                    start.Close();

                    if (started)
                    {
                        var delay = Task.Delay(500);
                        await delay.ConfigureAwait(false);

                        Application.Exit();
                    }
                });

                StartEvent = start;
            }
        }

        private void StudioBootstrapper_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exitWhenClosed && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        private void StudioBootstrapper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                return;

            DialogResult result = MessageBox.Show
            (
                this,

                "The installation has not finished yet!\n" +
                "Are you sure you want to close this window?",

                "Warning",

                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.No)
                return;

            e.Cancel = true;
        }
    }
}
