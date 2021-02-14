using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;
using System.Threading;

namespace RobloxStudioModManager
{
    public class StudioBootstrapper
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        private const string appSettingsXml =
            "<Settings>\n" +
            "   <ContentFolder>content</ContentFolder>\n" +
            "   <BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>";

        private const string UserAgent = "RobloxStudioModManager";
        public const string StartEvent = "RobloxStudioModManagerStart";

        private static readonly WebClient http;

        public event MessageEventHandler EchoFeed;
        public event MessageEventHandler StatusChanged;

        public event ChangeEventHandler<int> ProgressChanged;
        public event ChangeEventHandler<ProgressBarStyle> ProgressBarStyleChanged;

        private readonly RegistryKey mainRegistry;
        private readonly RegistryKey versionRegistry;
        private readonly RegistryKey pkgRegistry;
        private readonly RegistryKey fileRegistry;
        
        private Dictionary<string, string> newManifestEntries;
        private HashSet<string> writtenFiles;
        private FileManifest fileManifest;
        private string buildVersion;
        private string status;

        private int _progress = -1;
        private int _maxProgress = -1;
        private ProgressBarStyle _progressBarStyle;

        private static readonly IReadOnlyDictionary<string, string> knownRoots = new Dictionary<string, string>()
        {
            { "content-terrain.zip",   @"PlatformContent\pc\terrain\"  },
            { "content-textures3.zip", @"PlatformContent\pc\textures\" },
        };

        public int Progress
        {
            get => _progress;

            private set
            {
                if (_progress == value)
                    return;

                var change = new ChangeEventArgs<int>(value, "Progress");
                ProgressChanged?.Invoke(this, change);

                _progress = value;
            }
        }

        public int MaxProgress
        {
            get => _maxProgress;

            private set
            {
                if (_maxProgress == value)
                    return;

                if (value < Progress)
                    return;

                var change = new ChangeEventArgs<int>(value, "MaxProgress");
                ProgressChanged?.Invoke(this, change);

                _maxProgress = value;
            }
        }

        public ProgressBarStyle ProgressBarStyle
        {
            get => _progressBarStyle;

            private set
            {
                if (_progressBarStyle == value)
                    return;

                var change = new ChangeEventArgs<ProgressBarStyle>(value);
                ProgressBarStyleChanged?.Invoke(this, change);

                _progressBarStyle = value;
            }
        }

        public string Branch { get; set; } = "roblox";
        public string OverrideStudioDirectory { get; set; } = "";

        public bool CanShutdownStudio { get; set; } = true;
        public bool CanForceStudioShutdown { get; set; } = false;

        public bool ForceInstall { get; set; } = false;
        public bool SetStartEvent { get; set; } = false;
        public bool GenerateMetadata { get; set; } = false;
        public bool RemapExtraContent { get; set; } = false;
        public bool ApplyModManagerPatches { get; set; } = false;

        static StudioBootstrapper()
        {
            http = new WebClient();
            http.Headers.Set(HttpRequestHeader.UserAgent, UserAgent);
        }

        public StudioBootstrapper(RegistryKey workRegistry = null)
        {
            if (workRegistry == null)
                mainRegistry = Program.MainRegistry;
            else
                mainRegistry = workRegistry;

            Contract.Requires(mainRegistry != null);
            
            if (!mainRegistry.GetBool("Deprecate MD5"))
            {
                // The manifest registry needs to be reset.
                mainRegistry.SetValue("Deprecate MD5", true);
                mainRegistry.DeleteSubKeyTree("VersionData", false);
                mainRegistry.DeleteSubKeyTree("FileManifest", false);
                mainRegistry.DeleteSubKeyTree("PackageManifest", false);
            }

            versionRegistry = mainRegistry.GetSubKey("VersionData");
            pkgRegistry = mainRegistry.GetSubKey("PackageManifest");
            fileRegistry = mainRegistry.GetSubKey("FileManifest");
        }

        private void echo(string message)
        {
            var args = new MessageEventArgs(message);
            EchoFeed(this, args);
        }

        private void setStatus(string newStatus)
        {
            if (status != newStatus)
            {
                var args = new MessageEventArgs(newStatus);
                StatusChanged(this, args);
                status = newStatus;
            }
        }

        private static string getDirectory(params string[] paths)
        {
            string basePath = Path.Combine(paths);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return basePath;
        }

        private static string computeSignature(Stream source)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(source);

                string result = BitConverter
                    .ToString(hash)
                    .Replace("-", "")
                    .ToLower(Program.Format);

                return result;
            }
        }

        private static string computeSignature(ZipArchiveEntry entry)
        {
            string signature;

            using (Stream stream = entry.Open())
                signature = computeSignature(stream);

            return signature;
        }
        
        private void appendNewManifestEntry(string key, string value)
        {
            if (newManifestEntries == null)
                newManifestEntries = new Dictionary<string, string>();

            newManifestEntries[key] = value;
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

        public string GetLocalStudioDirectory()
        {
            if (!string.IsNullOrEmpty(OverrideStudioDirectory))
                return OverrideStudioDirectory;

            return GetStudioDirectory();
        }

        public string GetLocalStudioPath()
        {
            string studioDir = GetLocalStudioDirectory();
            return Path.Combine(studioDir, "RobloxStudioBeta.exe");
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

                action?.Invoke(process);
            }

            return studioProcs;
        }

        private void deleteUnusedFiles()
        {
            string studioDir = GetLocalStudioDirectory();
            setStatus("Deleting unused files...");

            if (newManifestEntries != null)
            {
                foreach (var pair in newManifestEntries)
                {
                    string key = pair.Key,
                           value = pair.Value;

                    if (fileManifest.ContainsKey(key))
                        continue;

                    fileRegistry.SetValue(key, value);
                    fileManifest[key] = value;
                }
            }

            var fileNames = fileRegistry
                .GetValueNames()
                .ToList();

            foreach (string fileName in fileNames)
            {
                string filePath = Path.Combine(studioDir, fileName);
                string lookupKey = fileName;

                if (fileName.StartsWith("Plugins\\", Program.StringFormat))
                    lookupKey = fileName.Substring(8);
                else if (fileName.StartsWith("Qml\\", Program.StringFormat))
                    lookupKey = fileName.Substring(4);

                if (!fileManifest.ContainsKey(lookupKey))
                {
                    if (File.Exists(filePath))
                    {
                        var info = new FileInfo(filePath);
                        string oldHash = fileRegistry.GetString(fileName);
                        
                        if (oldHash?.Length > 32)
                            if (info.Extension == ".dll" || info.Name == "qmldir")
                                continue;

                        echo($"Deleting unused file {fileName}");

                        try
                        {
                            fileRegistry.DeleteValue(fileName);
                            File.Delete(filePath);
                        }
                        catch
                        {
                            Console.WriteLine($"FAILED TO DELETE {fileName}");
                        }
                    }
                    else if (fileNames.Contains(fileName))
                    {
                        fileRegistry.DeleteValue(fileName);
                    }
                }
            }
        }

        public static async Task<ClientVersionInfo> GetTargetVersionInfo(string branch, string targetVersion, RegistryKey versionRegistry = null)
        {
            if (versionRegistry == null)
                versionRegistry = Program.VersionRegistry;

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
                var result = GetCurrentVersionInfo(branch, versionRegistry);
                return await result.ConfigureAwait(false);
            }

            return new ClientVersionInfo()
            {
                Guid = target.VersionGuid,
                Version = target.VersionId
            };
        }

        public static async Task<ClientVersionInfo> GetCurrentVersionInfo(string branch, RegistryKey versionRegistry = null, string fastGuid = "", string targetVersion = "")
        {
            Contract.Requires(fastGuid != null);

            if (versionRegistry == null)
                versionRegistry = Program.VersionRegistry;

            if (!string.IsNullOrEmpty(targetVersion))
            {
                var result = GetTargetVersionInfo(branch, targetVersion, versionRegistry);
                return await result.ConfigureAwait(false);
            }

            string binaryType = GetStudioBinaryType();
            bool is64Bit = Environment.Is64BitOperatingSystem;

            if (branch == "roblox")
            {
                var result = ClientVersionInfo.Get(binaryType);
                return await result.ConfigureAwait(false);
            }

            if (string.IsNullOrEmpty(fastGuid))
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

            return filePath.Replace('/', '\\');
        }

        private bool shouldFetchPackage(Package package)
        {
            string pkgName = package.Name;
            var pkgInfo = pkgRegistry.GetSubKey(pkgName);

            string oldSig = pkgInfo.GetString("Signature");
            string newSig = package.Signature;

            if (oldSig == newSig && !ForceInstall)
            {
                int fileCount = pkgInfo.GetInt("NumFiles");
                echo($"Package '{pkgName}' hasn't changed between builds, skipping.");

                MaxProgress += fileCount;
                Progress += fileCount;

                return false;
            }

            return true;
        }

        private async Task<bool> packageExists(Package package)
        {
            try
            {
                echo($"Verifying availability of: {package.Name}");

                string pkgName = package.Name;
                var zipFileUrl = new Uri($"https://s3.amazonaws.com/setup.{Branch}.com/{buildVersion}-{pkgName}");

                var request = WebRequest.Create(zipFileUrl) as HttpWebRequest;
                request.Headers.Set("UserAgent", UserAgent);
                request.Method = "HEAD";

                var response = await request
                    .GetResponseAsync() 
                    .ConfigureAwait(false)
                    as HttpWebResponse;

                var statusCode = response.StatusCode;
                response.Close();

                return (statusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        private async Task<byte[]> installPackage(Package package)
        {
            byte[] result = null;
            string pkgName = package.Name;
            string zipFileUrl = $"https://s3.amazonaws.com/setup.{Branch}.com/{buildVersion}-{pkgName}";

            using (var localHttp = new WebClient())
            {
                localHttp.Headers.Set("UserAgent", UserAgent);
                echo($"Installing package {zipFileUrl}");
                MaxProgress++;
                
                try
                {
                    var getFile = localHttp.DownloadDataTaskAsync(zipFileUrl);
                    byte[] fileContents = await getFile.ConfigureAwait(false);
                    
                    // If the size of the file we downloaded does not match the packed 
                    // size specified in the manifest, then this file isn't valid.

                    if (fileContents.Length != package.PackedSize)
                        throw new InvalidDataException($"{pkgName} expected packed size: {package.PackedSize} but got: {fileContents.Length}");

                    Progress++;
                    result = fileContents;
                }
                catch (Exception e)
                {
                    echo($"Error while fetching package {pkgName}:");
                    echo(e.Message);
                }
            }

            return result;
        }

        private void extractPackage(Package package)
        {
            Contract.Requires(package.Data != null);
            string pkgName = package.Name;

            var pkgInfo = pkgRegistry.GetSubKey(pkgName);
            string studioDir = GetLocalStudioDirectory();

            string downloads = getDirectory(studioDir, "downloads");
            string zipExtractPath = Path.Combine(downloads, pkgName);

            File.WriteAllBytes(zipExtractPath, package.Data);

            using (var archive = ZipFile.OpenRead(zipExtractPath))
            {
                var deferred = new Dictionary<ZipArchiveEntry, string>();

                int numFiles = archive.Entries
                    .Select(entry => entry.FullName)
                    .Where(name => !name.EndsWith("/", Program.StringFormat))
                    .Count();

                string localRootDir = null;
                MaxProgress += numFiles;

                if (knownRoots.ContainsKey(pkgName))
                    localRootDir = knownRoots[pkgName];

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Length == 0)
                    {
                        Progress++;
                        continue;
                    }
                    
                    string newFileSig = null;
                    string entryPath = entry.FullName.Replace('/', '\\');

                    // If we have figured out what our root directory is, try to resolve
                    // what the signature of this file is.

                    if (localRootDir != null)
                    {
                        bool hasEntryPath = fileManifest.ContainsKey(entryPath);
                        
                        // If we can't find this file in the signature lookup table,
                        // try appending the local directory to it. This resolves some
                        // edge cases relating to the fixFilePath function above.

                        if (!hasEntryPath)
                        {
                            entryPath = localRootDir + entryPath;
                            hasEntryPath = fileManifest.ContainsKey(entryPath);
                        }

                        // If we can find this file path in the file manifest, then we will
                        // use its pre-computed signature to check if the file has changed.

                        newFileSig = hasEntryPath ? fileManifest[entryPath] : null;
                    }
                    else
                    {
                        var query = fileManifest.Where(pair => pair.Key.EndsWith(entryPath, Program.StringFormat));
                        newFileSig = query.Any() ? query.First().Value : null;
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

                                if (filePath != file)
                                    appendNewManifestEntry(filePath, newFileSig);
                                
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

                        if (string.IsNullOrEmpty(localRootDir))
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

                // Process any files that we deferred from writing immediately.
                foreach (ZipArchiveEntry entry in deferred.Keys)
                {
                    string file = entry.FullName;
                    string newFileSig = deferred[entry];

                    if (localRootDir != null)
                        file = localRootDir + file;

                    if (!fileManifest.ContainsKey(file))
                        appendNewManifestEntry(file, newFileSig);
                    
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

            if (filePath != file)
                appendNewManifestEntry(filePath, newFileSig);

            if (!ForceInstall)
            {
                string oldFileSig = fileRegistry.GetString(filePath);

                if (oldFileSig == newFileSig)
                {
                    Progress++;
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

            Progress++;
            writtenFiles.Add(filePath);
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

                    foreach (Process p in running)
                    {
                        if (CanForceStudioShutdown)
                        {
                            tryToKillProcess(p);
                            continue;
                        }

                        SetForegroundWindow(p.MainWindowHandle);
                        FlashWindow(p.MainWindowHandle, true);

                        var delay = Task.Delay(50);
                        p.CloseMainWindow();

                        await delay.ConfigureAwait(true);
                    }

                    List<Process> runningNow = null;

                    const int retries = 10;
                    const int granularity = 300;

                    Progress = 0;
                    MaxProgress = retries * granularity;
                    ProgressBarStyle = ProgressBarStyle.Continuous;

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
                            Progress += granularity;

                            await delay.ConfigureAwait(true);
                        }
                    }

                    if (runningNow.Count > 0 && !safeToContinue)
                    {
                        DialogResult result = DialogResult.OK;

                        if (mainRegistry == Program.MainRegistry)
                        {
                            result = MessageBox.Show
                            (
                                "All Roblox Studio processes need to be closed in order to update Roblox Studio!\n" +
                                "Press Ok once you've saved your work, or\n" +
                                "Press Cancel to skip this update temporarily.",

                                "Notice",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Warning
                            );
                        }

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

        public async Task<bool> Bootstrap(string targetVersion = "")
        {
            setStatus("Checking for updates");
            echo("Checking build installation...");

            string currentVersion = versionRegistry.GetString("VersionGuid");
            string currentBranch;

            if (mainRegistry.Name.EndsWith(Branch, Program.StringFormat))
                currentBranch = Branch;
            else
                currentBranch = mainRegistry.GetString("BuildBranch", "roblox");

            bool shouldInstall = (ForceInstall || currentBranch != Branch);
            ClientVersionInfo versionInfo = null;

            var getFastVersion = GetFastVersionGuid(currentBranch);
            string fastVersion = await getFastVersion.ConfigureAwait(true);

            if (!shouldInstall)
                shouldInstall = (fastVersion != currentVersion);

            string versionOverload = versionRegistry.GetString("VersionOverload");

            if (targetVersion != versionOverload)
                shouldInstall = true;

            if (shouldInstall)
            {
                if (currentBranch != "roblox")
                    echo("Possible update detected, verifying...");

                var getVersionInfo = GetCurrentVersionInfo(currentBranch, versionRegistry, fastVersion, targetVersion);
                versionInfo = await getVersionInfo.ConfigureAwait(true);

                if (targetVersion == versionOverload)
                    if (fastVersion != versionInfo.Guid)
                        shouldInstall = false;

                buildVersion = versionInfo.Guid;
            }
            else
            {
                buildVersion = fastVersion;
            }

            if (currentVersion != buildVersion || shouldInstall)
            {
                echo("This build needs to be installed!");
                bool studioClosed = true;

                if (CanShutdownStudio)
                {
                    var closeStudio = shutdownStudioProcesses();
                    studioClosed = await closeStudio.ConfigureAwait(true);
                }
                
                if (studioClosed)
                {
                    string binaryType = GetStudioBinaryType();
                    string studioDir = GetLocalStudioDirectory();
                    string versionId = versionInfo.Version;

                    setStatus($"Installing Version {versionId} of Roblox Studio...");
                    echo("Grabbing package manifest...");

                    var pkgManifest = await PackageManifest
                        .Get(Branch, buildVersion)
                        .ConfigureAwait(true);

                    echo("Grabbing file manifest...");

                    fileManifest = await FileManifest
                        .Get(Branch, buildVersion, RemapExtraContent)
                        .ConfigureAwait(true);

                    var taskQueue = new List<Task>();
                    writtenFiles = new HashSet<string>();

                    Progress = 0;
                    MaxProgress = 0;
                    ProgressBarStyle = ProgressBarStyle.Continuous;

                    foreach (Package package in pkgManifest)
                    {
                        package.ShouldInstall = shouldFetchPackage(package);

                        if (!package.ShouldInstall)
                        {
                            package.Exists = true;
                            continue;
                        }

                        Task verify = Task.Run(async () =>
                        {
                            var doesExist = packageExists(package);
                            package.Exists = await doesExist.ConfigureAwait(false);
                        });

                        taskQueue.Add(verify);
                    }

                    await Task
                       .WhenAll(taskQueue)
                       .ConfigureAwait(true);

                    taskQueue.Clear();

                    foreach (Package package in pkgManifest)
                    {
                        if (!package.Exists)
                        {
                            setStatus("Installation Failed!");
                            echo($"ERROR WHILE INSTALLING: Package {package.Name} could not be fetched! Skipping installation for now.");

                            Progress = 1;
                            MaxProgress = 1;
                            ProgressBarStyle = ProgressBarStyle.Marquee;

                            var timeout = Task.Delay(2000);
                            await timeout.ConfigureAwait(false);

                            return false;
                        }

                        if (!package.ShouldInstall)
                            continue;

                        var installer = Task.Run(async () =>
                        {
                            bool success = true;

                            try
                            {
                                var install = installPackage(package);
                                package.Data = await install.ConfigureAwait(false);
                                extractPackage(package);
                            }
                            catch (Exception e)
                            {
                                echo($"ERROR: {e.Message}");
                                success = false;
                            }

                            return success;
                        });

                        taskQueue.Add(installer);
                    }

                    await Task
                        .WhenAll(taskQueue)
                        .ConfigureAwait(true);

                    foreach (var task in taskQueue)
                    {
                        bool passed = true;

                        if (task is Task<bool> boolTask)
                            if (!boolTask.Result)
                                passed = false;

                        if (task.IsFaulted)
                            passed = false;

                        if (!passed)
                        {
                            setStatus("Installation Failed!");
                            echo("One or more packages failed to install correctly! Skipping update for now...");

                            await Task
                                .Delay(500)
                                .ConfigureAwait(false);

                            return false;
                        }
                    }
                    
                    taskQueue.Clear();

                    foreach (Package package in pkgManifest)
                    {
                        if (!package.ShouldInstall)
                            continue;

                        var extract = Task.Run(() => extractPackage(package));
                        taskQueue.Add(extract);
                    }

                    await Task
                        .WhenAll(taskQueue)
                        .ConfigureAwait(true);

                    echo("Writing AppSettings.xml...");

                    string appSettings = Path.Combine(studioDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, appSettingsXml);

                    setStatus("Deleting unused files...");
                    deleteUnusedFiles();
                    
                    if (GenerateMetadata)
                    {
                        echo("Writing metadata...");

                        string pkgManifestPath = Path.Combine(studioDir, "rbxPkgManifest.txt");
                        File.WriteAllText(pkgManifestPath, pkgManifest.RawData);

                        string fileManifestPath = Path.Combine(studioDir, "rbxManifest.txt");
                        File.WriteAllText(fileManifestPath, fileManifest.RawData);

                        string versionPath = Path.Combine(studioDir, "version.txt");
                        File.WriteAllText(versionPath, versionId);

                        string versionGuidPath = Path.Combine(studioDir, "version-guid.txt");
                        File.WriteAllText(versionGuidPath, buildVersion);

                        echo("Dumping API...");

                        string studioPath = GetLocalStudioPath();
                        string apiPath = Path.Combine(studioDir, "API-Dump.json");

                        var dumpApi = Process.Start(studioPath, $"-API \"{apiPath}\"");
                        dumpApi.WaitForExit();
                    }

                    ProgressBarStyle = ProgressBarStyle.Marquee;

                    if (mainRegistry.Name != Branch)
                        mainRegistry.SetValue("BuildBranch", Branch);

                    versionRegistry.SetValue("Version", versionId);
                    versionRegistry.SetValue("VersionGuid", buildVersion);
                    versionRegistry.SetValue("VersionOverload", targetVersion);
                }
                else
                {
                    ProgressBarStyle = ProgressBarStyle.Marquee;
                    echo("Update cancelled. Launching on current branch and version.");
                }
            }
            else
            {
                echo("This version of Roblox Studio has been installed!");
            }

            // Only update the registry protocols if the main registry
            // is the global one assigned to the program itself.

            if (mainRegistry == Program.MainRegistry)
            {
                setStatus("Configuring Roblox Studio...");
                echo("Updating registry protocols...");

                Program.UpdateStudioRegistryProtocols();
            }

            if (ApplyModManagerPatches && string.IsNullOrEmpty(OverrideStudioDirectory))
            {
                echo("Applying flag configuration...");
                FlagEditor.ApplyFlags();

                echo("Patching explorer icons...");

                await ClassIconEditor
                    .PatchExplorerIcons()
                    .ConfigureAwait(true);

                // Secret feature only for me :(
                // Feel free to patch in your own thing if you want.

#               if ROBLOX_INTERNAL
                var rbxInternal = Task.Run(() => RobloxInternal.Patch(this));
                await rbxInternal.ConfigureAwait(false);
#               endif
            }

            setStatus("Starting Roblox Studio...");
            echo("Roblox Studio is up to date!");

            if (SetStartEvent)
            {
                _ = Task.Run(async () =>
                {
                    var start = new SystemEvent(StartEvent);

                    bool started = await start
                        .WaitForEvent()
                        .ConfigureAwait(true);

                    start.Close();

                    if (started)
                    {
                        var delay = Task.Delay(3000);
                        await delay.ConfigureAwait(false);

                        Application.Exit();
                    }

                    start.Dispose();
                });
            }

            return true;
        }
    }
}
