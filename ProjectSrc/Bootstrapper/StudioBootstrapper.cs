using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#if !CLI
using System.Windows.Forms;
#endif

using Newtonsoft.Json;
using RobloxDeployHistory;
using Konscious.Security.Cryptography;
using System.Runtime.Versioning;
using System.Collections.Concurrent;

namespace RobloxStudioModManager
{
    public delegate void MessageFeed(string message);

    public enum BarStyle
    {
        Continuous,
        Marquee,
    }

    public partial class StudioBootstrapper
    {
#if !CLI
        [LibraryImport("user32.dll")]
        [SupportedOSPlatform("windows")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [SupportedOSPlatform("windows")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FlashWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bInvert);
#endif

    #if !CLI
        private static string AppSettings_XML;
        private static string OAuth2Config_JSON;
    #endif
        private const string UserAgent = "RobloxStudioModManager";

        public event MessageFeed EchoFeed;
        public event MessageFeed StatusFeed;

        private readonly IBootstrapperState mainState;
        private readonly VersionManifest versionData;
        private readonly ChannelManifest channelData;
        private readonly SortedDictionary<string, string> fileRegistry;
        private readonly SortedDictionary<string, PackageState> pkgRegistry;

        private Dictionary<string, string[]> bySignature;
        private Dictionary<string, string> newManifestEntries;
        private ConcurrentBag<string> writtenFiles;
        private FileManifest fileManifest;
        private string buildVersion;
        private string status;

        public static readonly List<string> BadManifests = new List<string>();
        public static readonly Dictionary<string, string> KnownRoots = new Dictionary<string, string>();

        public long Progress = 0;
        public long MaxProgress = 0;
        public BarStyle ProgressBarStyle = BarStyle.Continuous;

        public object ProgressLock = new object();

        public string OverrideStudioDirectory { get; set; } = "";
        public string OverrideGuid { get; set; } = "";

        public bool CanShutdownStudio { get; set; } = true;
        public bool CanForceStudioShutdown { get; set; } = false;

        public bool ForceInstall { get; set; } = false;
        public bool GenerateMetadata { get; set; } = false;
        public bool RemapExtraContent { get; set; } = false;
        public bool ApplyModManagerPatches { get; set; } = false;
        
        public StudioBootstrapper(IBootstrapperState state = null)
        {
            mainState = state ?? Program.State;
            versionData = mainState.VersionData;
            channelData = mainState.ChannelData;
            fileRegistry = mainState.FileManifest;
            pkgRegistry = mainState.PackageManifest;
        }

        private void echo(string message)
        {
            EchoFeed.Invoke(message);
        }

        private void setStatus(string newStatus)
        {
            if (status != newStatus)
            {
                StatusFeed?.Invoke(newStatus);
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
            using var blake2b = new HMACBlake2B(16 * 8);
            blake2b.TransformBlock(Array.Empty<byte>(), 0, 0, null, 0);

            byte[] hash = blake2b.ComputeHash(source);
            return Convert.ToHexStringLower(hash);
        }

        private static string computeSignature(ZipArchiveEntry entry)
        {
            string signature;

            using (var stream = entry.Open())
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
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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

#if !CLI
        private static void tryToKillProcess(Process process)
        {
            try
            {
                process.Kill();
            }
            catch (Win32Exception)
            {
                Console.WriteLine($"Cannot terminate process {process.Id}!");
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
#endif

        internal static string ResolveFilePath(string root, string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');

            if (string.IsNullOrWhiteSpace(normalized) || normalized.StartsWith("/", StringComparison.Ordinal)
                || normalized.Contains(':') || normalized.Split('/').Any(part => part == ".." || part == "."))
                throw new InvalidDataException("Unsafe package path: " + relativePath);

            root = Path.GetFullPath(root);
            string path = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));

            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            string prefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;

            if (!path.StartsWith(prefix, comparison))
                throw new InvalidDataException("Package path escapes the output directory: " + relativePath);
            
            for (string current = path; current != null; current = Path.GetDirectoryName(current))
            {
                if (new FileInfo(current).LinkTarget != null || (File.Exists(current) || Directory.Exists(current))
                    && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Output paths must not traverse symbolic links: " + current);
            }

            return path;
        }

        private void deleteUnusedFiles()
        {
            string studioDir = GetLocalStudioDirectory();

            if (newManifestEntries != null)
            {
                foreach (var pair in newManifestEntries)
                {
                    string key = pair.Key,
                           value = pair.Value;

                    if (key == null || value == null)
                        continue;

                    if (fileManifest.ContainsKey(key))
                        continue;

                    fileRegistry[key] = value;
                    fileManifest[key] = value;
                }
            }

            var fileNames = fileRegistry.Keys.ToList();

            lock (ProgressLock)
            {
                Progress = 0;
                MaxProgress = fileNames.Count;
                ProgressBarStyle = fileNames.Count > 0 ? BarStyle.Continuous : BarStyle.Marquee;
            }

            foreach (string fileName in fileNames)
            {
                Interlocked.Increment(ref Progress);
                if (fileName == null)
                    continue;

                string lookupKey = fileName;

                if (!fileManifest.ContainsKey(fileName))
                    foreach (string pkgName in BadManifests)
                        if (fileName.StartsWith(pkgName, Program.StringFormat))
                            lookupKey = fileName.Substring(pkgName.Length + 1);

                if (!fileManifest.ContainsKey(lookupKey))
                {
                    string filePath = ResolveFilePath(studioDir, fileName);
                    if (File.Exists(filePath))
                    {
                        var info = new FileInfo(filePath);
                        string oldHash = fileRegistry[fileName];
                        
                        if (oldHash?.Length > 32)
                            if (info.Extension == ".dll" || info.Name == "qmldir" || filePath.Contains("api-docs"))
                                continue;

                        echo($"Deleting unused file {fileName}");
                        fileRegistry.Remove(fileName);
                        File.Delete(filePath);
                    }
                    else
                    {
                        fileRegistry.Remove(fileName);
                    }
                }
            }
        }

        public static async Task<ClientVersionInfo> GetTargetVersionInfo(string targetVersion = "", VersionManifest versionData = null, ChannelManifest channelData = null)
        {
            if (versionData == null)
                versionData = Program.State.VersionData;

            if (channelData == null)
                channelData = Program.State.ChannelData;

            var logData = await StudioDeployLogs.Get(Program.AllowUnsupportedVersions, channelData.ChannelName, channelData.ChannelToken);
            HashSet<DeployLog> targets = logData.CurrentLogs;

            DeployLog target = targets
                .FirstOrDefault(log => log.VersionId == targetVersion);
            
            if (target == null)
                throw new InvalidOperationException("Unknown Studio version: " + targetVersion);

            return new ClientVersionInfo(target);
        }

        public static async Task<ClientVersionInfo> GetCurrentVersionInfo(string targetVersion = "", VersionManifest versionData = null, ChannelManifest channelData = null)
        {
            if (versionData == null)
                versionData = Program.State.VersionData;

            if (channelData == null)
                channelData = Program.State.ChannelData;

            if (!string.IsNullOrEmpty(targetVersion))
            {
                var result = GetTargetVersionInfo(targetVersion, versionData, channelData);
                return await result.ConfigureAwait(false);
            }

#if CLI
            var latest = await ClientVersionInfo.Get(channelData.ChannelName, "WindowsStudio64", channelData.ChannelToken);

            if (!latest.Success || string.IsNullOrEmpty(latest.VersionGuid))
                throw new InvalidOperationException("Could not resolve the requested Studio channel.");
            
            return latest;
#else
            var logData = await StudioDeployLogs.Get(Program.AllowUnsupportedVersions, channelData.ChannelName, channelData.ChannelToken);
            var build = logData.CurrentLogs.LastOrDefault();
            var info = new ClientVersionInfo(build);

            if (build != null)
                versionData.LatestGuid_x64 = build.VersionGuid;
            
            return info;
#endif
        }

        private string fixFilePath(string pkgName, string filePath)
        {
            string pkgDir = pkgName.Replace(".zip", "");

            if (BadManifests.Contains(pkgDir))
                if (!filePath.StartsWith(pkgDir, Program.StringFormat))
                    filePath = pkgDir + '\\' + filePath;

            if (RemapExtraContent)
                filePath = filePath.Replace("ExtraContent", "content");

            return filePath.Replace('/', '\\');
        }

        private bool shouldFetchPackage(Package package)
        {
            string pkgName = package.Name;

            if (!pkgRegistry.TryGetValue(pkgName, out var pkgInfo))
            {
                pkgInfo = new PackageState();
                pkgRegistry[pkgName] = pkgInfo;
            }

            string oldSig = pkgInfo.Signature;
            string newSig = package.Signature;

            if (oldSig == newSig && !ForceInstall)
            {
                echo($"Package '{pkgName}' hasn't changed between builds, skipping.");
                return false;
            }

            return true;
        }

        private async Task<bool> packageExists(Package package)
        {
            echo($"Verifying availability of: {package.Name}");

            var zipFileUrl = new Uri($"{Program.BaseUrl}/channel/common/{buildVersion}-{package.Name}");
            var message = new HttpRequestMessage(HttpMethod.Head, zipFileUrl);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("UserAgent", UserAgent);

            using var response = await http.SendAsync(message);
            return response.StatusCode == HttpStatusCode.OK;
        }

        private async Task<byte[]> installPackage(Package package)
        {
            byte[] result = null;
            string pkgName = package.Name;
            string zipFileUrl = $"{Program.BaseUrl}/channel/common/{buildVersion}-{pkgName}";

            using (var localHttp = new HttpClient())
            {
                localHttp.DefaultRequestHeaders.Add("UserAgent", UserAgent);
                echo($"Installing package {zipFileUrl}");

                // Download the package as a stream.
                using var response = await localHttp.GetAsync(zipFileUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                result = await ReadPackageContents(stream, package.PackedSize, (bytesRead) => Interlocked.Add(ref Progress, bytesRead));
            }

            return result;
        }

        internal static async Task<byte[]> ReadPackageContents(Stream stream, int expectedSize, Action<int> onBytesRead = null)
        {
            using var output = new MemoryStream();
            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                if (output.Length + bytesRead > expectedSize)
                    throw new InvalidDataException("Downloaded package exceeds the manifest size.");
                
                await output.WriteAsync(buffer, 0, bytesRead);
                onBytesRead?.Invoke(bytesRead);
            }

            if (output.Length != expectedSize)
                throw new InvalidDataException($"Expected package size {expectedSize}, got {output.Length}.");
            
            return output.ToArray();
        }

        private void extractPackage(Package package)
        {
            var data = package.Data;
            string pkgName = package.Name;
            string studioDir = GetLocalStudioDirectory();

            if (!pkgRegistry.TryGetValue(pkgName, out var pkgInfo))
            {
                pkgInfo = new PackageState();
                pkgRegistry[pkgName] = pkgInfo;
            }
            
            string downloads = getDirectory(studioDir, "downloads");
            string zipExtractPath = ResolveFilePath(downloads, pkgName);

            echo($"Writing {zipExtractPath}...");
            File.WriteAllBytes(zipExtractPath, data);

            using (var archive = ZipFile.OpenRead(zipExtractPath))
            {
                var deferred = new Dictionary<ZipArchiveEntry, string>();

                lock (ProgressLock)
                    MaxProgress += archive.Entries.Count;

                int numFiles = archive.Entries
                    .Select(entry => entry.FullName)
                    .Where(name => !name.EndsWith("/", Program.StringFormat))
                    .Count();

                KnownRoots.TryGetValue(pkgName, out string localRootDir);

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    bool skip = false;
                    Interlocked.Increment(ref Progress);

                    if (entry.FullName.EndsWith("/", StringComparison.Ordinal))
                        skip = true;

                    if (entry.Name.EndsWith(".robloxrc", Program.StringFormat))
                        skip = true;

                    if (entry.Name.EndsWith(".luarc", Program.StringFormat))
                        skip = true;

                    if (skip)
                        continue;

                    string newFileSig = null;
                    string entryPath = entry.FullName.Replace('/', '\\');

                    // If we have figured out what our root directory is, try to resolve
                    // what the signature of this file is.

                    if (localRootDir != null)
                    {
                        // Append local directory to our path.
                        var manifestKey = localRootDir + entryPath;
                        bool hasEntryPath = fileManifest.ContainsKey(manifestKey);
                        
                        if (!hasEntryPath)
                        {
                            // If we can't find this file in the signature lookup table,
                            // try falling back to the entryPath.

                            manifestKey = entryPath;
                            hasEntryPath = fileManifest.ContainsKey(manifestKey);
                        }

                        // If we can find this file path in the file manifest, then we will
                        // use its pre-computed signature to check if the file has changed.

                        newFileSig = hasEntryPath ? fileManifest[manifestKey] : null;
                    }
                    else if (!fileManifest.TryGetValue(entryPath, out newFileSig))
                    {
                        var query = fileManifest.SkipWhile(pair => !pair.Key.EndsWith(entryPath, Program.StringFormat)).Take(1);
                        newFileSig = query.Any() ? query.First().Value : null;
                    }

                    // If we couldn't pre-determine the file signature from the manifest,
                    // then we have to compute it manually. This is slower.

                    if (newFileSig == null)
                        newFileSig = computeSignature(entry);

                    // Now check what files this signature corresponds with.
                   
                    if (bySignature.TryGetValue(newFileSig, out var files))
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

                pkgInfo.Signature = package.Signature;
                pkgInfo.NumFiles = numFiles;
            }
        }

        private void WritePackageFile(string studioDir, string pkgName, string file, string newFileSig, ZipArchiveEntry entry)
        {
            if (entry.Name == "")
                return;

            string filePath = fixFilePath(pkgName, file);

            if (writtenFiles.Contains(filePath))
                return;

            if (filePath != file)
                appendNewManifestEntry(filePath, newFileSig);

            if (!ForceInstall)
            {
                if (!fileRegistry.TryGetValue(filePath, out string oldFileSig))
                    oldFileSig = "";

                if (oldFileSig == newFileSig)
                {
                    return;
                }
            }

            string extractPath = ResolveFilePath(studioDir, filePath);
            string extractDir = Path.GetDirectoryName(extractPath);

            getDirectory(extractDir);

            if (File.Exists(extractPath))
                File.Delete(extractPath);

            echo($"Writing {filePath}...");
            entry.ExtractToFile(extractPath);

            fileRegistry[filePath] = newFileSig;
            writtenFiles.Add(filePath);
        }

    #if !CLI
        private async Task<bool> shutdownStudioProcesses()
        {
            bool cancelled = false;
            bool safeToContinue = false;
            setStatus("Shutting down Roblox Studio...");

            // Check if studio processes exist & prompt the user to restart studio
            List<Process> initialRunning = GetRunningStudioProcesses();

            if (initialRunning.Count > 0 && OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            {
                DialogResult result = DialogResult.OK;

                if (mainState == Program.State)
                {
                    result = MessageBox.Show
                    (
                        "All Roblox Studio processes need to be closed in order to update Roblox Studio!\n" +
                        "Press Ok once you've saved your work, or\n" +
                        "Press Cancel to skip this update temporarily.",

                        "Notice",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly
                    );
                }

                if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            // Close all running processes
            while (!safeToContinue)
            {
                List<Process> running = GetRunningStudioProcesses();

                // No processs open, good to go
                if (running.Count <= 0)
                {
                    safeToContinue = true;
                    continue;
                }

                // Handle process termination
                foreach (Process p in running)
                {
                    if (CanForceStudioShutdown)
                    {
                        tryToKillProcess(p);
                        continue;
                    }

                    if (OperatingSystem.IsWindows())
                    {
                        SetForegroundWindow(p.MainWindowHandle);
                        FlashWindow(p.MainWindowHandle, true);
                    }

                    var delay = Task.Delay(50);
                    p.CloseMainWindow();

                    await delay.ConfigureAwait(true);
                }

                const int retries = 10;
                const int granularity = 300;

                Progress = 0;
                MaxProgress = retries * granularity;
                ProgressBarStyle = BarStyle.Continuous;

                for (int i = 0; i < retries; i++)
                {
                    List<Process> runningNow = GetRunningStudioProcesses();

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
            }

            return !cancelled;
        }
#endif

        public async Task<bool> Bootstrap(string targetVersion = "")
        {
            setStatus("Checking for updates");
            echo("Checking build installation...");

            string currentVersion = versionData.VersionGuid;
            var getVersionInfo = OverrideGuid != "" && targetVersion != ""
                ? Task.FromResult(new ClientVersionInfo(targetVersion, OverrideGuid))
                : GetCurrentVersionInfo(targetVersion, versionData, channelData);
            ClientVersionInfo versionInfo = await getVersionInfo.ConfigureAwait(true);

            if (OverrideGuid != "")
                versionInfo = new ClientVersionInfo(versionInfo.Version, OverrideGuid);

            string studioDir = GetLocalStudioDirectory();
            buildVersion = versionInfo.VersionGuid;
            
            if (currentVersion != buildVersion || ForceInstall)
            {
                echo("This build needs to be installed!");
                bool studioClosed = true;

#if !CLI
                if (CanShutdownStudio)
                {
                    var closeStudio = shutdownStudioProcesses();
                    studioClosed = await closeStudio.ConfigureAwait(true);
                }
#endif
                
                if (studioClosed)
                {
                    const string binaryType = "WindowsStudio64";
                    string versionId = versionInfo.Version;

                    var installVersion = versionId
                        .Split('.')
                        .Select(int.Parse)
                        .Skip(1)
                        .First();

                    KnownRoots.Clear();
                    BadManifests.Clear();
                    setStatus($"Fetching Known Roots...");

                    using (var http = new HttpClient())
                    {
                        var json = await http.GetStringAsync(Program.BaseConfigUrl + "KnownRoots.json");
                        var knownRoots = JsonConvert.DeserializeObject<Dictionary<string, KnownRoot>>(json);

                        foreach (var pair in knownRoots)
                        {
                            string name = pair.Key;
                            var knownRoot = pair.Value;

                            if (installVersion < knownRoot.MinVersion)
                                continue;

                            string file = name + ".zip";
                            string extractPath = knownRoot.ExtractTo;

                            if (!string.IsNullOrEmpty(extractPath))
                                extractPath += '/';

                            if (knownRoot.BadManifest)
                                BadManifests.Add(name);

                            KnownRoots.Add(file, extractPath.Replace('/', '\\'));
                        }
                    }

                    setStatus($"Installing Packages...");
                    echo("Grabbing package manifest...");

                    var pkgManifest = await PackageManifest
                        .Get(versionInfo)
                        .ConfigureAwait(true);

                    echo("Grabbing file manifest...");

                    fileManifest = await FileManifest
                        .Get(versionInfo, RemapExtraContent)
                        .ConfigureAwait(true);

                    var taskQueue = new List<Task>();
                    writtenFiles = new ConcurrentBag<string>();

                    bySignature =
                        (from pair in fileManifest
                         let key = pair.Key
                         let value = pair.Value
                         group key by value)
                        .ToDictionary(group => group.Key, group => group.ToArray());

                    lock (ProgressLock)
                    {
                        Progress = 0;
                        MaxProgress = 0;
                        ProgressBarStyle = BarStyle.Marquee;
                    }

                    // Verify all of these packages are available to install.
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

                    lock (ProgressLock)
                    {
                        Progress = 0;
                        MaxProgress = pkgManifest.Where(package => package.ShouldInstall)
                            .Sum(package => (long)package.PackedSize);
                        ProgressBarStyle = MaxProgress > 0 ? BarStyle.Continuous : BarStyle.Marquee;
                    }

                    // Install packages, abort if any of them don't exist.
                    foreach (Package package in pkgManifest)
                    {
                        if (!package.Exists)
                        {
                            setStatus("Installation Failed!");
                            echo($"ERROR WHILE INSTALLING: Package {package.Name} could not be fetched! Skipping installation for now.");

                            Progress = 1;
                            MaxProgress = 1;
                            ProgressBarStyle = BarStyle.Marquee;

                            var timeout = Task.Delay(2000);
                            await timeout.ConfigureAwait(false);

                            return false;
                        }

                        if (!package.ShouldInstall)
                            continue;

                        var installer = Task.Run(async () =>
                        {
                            var install = installPackage(package);
                            package.Data = await install.ConfigureAwait(false);
                            return true;
                        });

                        taskQueue.Add(installer);
                    }

                    await Task
                        .WhenAll(taskQueue)
                        .ConfigureAwait(true);

                    // Make sure the packages were installed without errors.
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
                    
                    // Extract all of the packages
                    setStatus($"Extracting Packages...");
                    taskQueue.Clear();

                    lock (ProgressLock)
                    {
                        Progress = 0;
                        MaxProgress = 0;
                        ProgressBarStyle = BarStyle.Continuous;
                    }

                    foreach (Package package in pkgManifest)
                    {
                        if (package.ShouldInstall)
                        {
#if CLI
                            extractPackage(package);
#else
                            var extract = Task.Run(() => extractPackage(package));
                            taskQueue.Add(extract);
#endif
                        }
                    }

#if !CLI
                    await Task
                        .WhenAll(taskQueue)
                        .ConfigureAwait(true);
#endif
                    setStatus("Deleting unused files...");
                    lock (ProgressLock)
                    {
                        Progress = 0;
                        MaxProgress = 0;
                        ProgressBarStyle = BarStyle.Marquee;
                    }
                    await Task.Run(deleteUnusedFiles).ConfigureAwait(true);
                    
                    if (GenerateMetadata)
                    {
                        echo("Writing metadata...");

                        string pkgManifestPath = ResolveFilePath(studioDir, "rbxPkgManifest.txt");
                        File.WriteAllText(pkgManifestPath, pkgManifest.RawData);

                        string fileManifestPath = ResolveFilePath(studioDir, "rbxManifest.txt");
                        File.WriteAllText(fileManifestPath, fileManifest.RawData);

                        string versionPath = ResolveFilePath(studioDir, "version.txt");
                        File.WriteAllText(versionPath, versionId);

                        string versionGuidPath = ResolveFilePath(studioDir, "version-guid.txt");
                        File.WriteAllText(versionGuidPath, buildVersion);

#if !CLI
                        echo("Dumping API...");
                        await Task.Delay(2000);

                        string studioPath = GetLocalStudioPath();
                        string apiPath = Path.Combine(studioDir, "API-Dump.json");
                        string fullApiPath = Path.Combine(studioDir, "Full-API-Dump.json");
                        string apiV2Path = Path.Combine(studioDir, "UNSTABLE___API-Dump-2.json");

                        var dumpApi = Process.Start(studioPath, $"-API \"{apiPath}\"");
                        var dumpApiV2 = Process.Start(studioPath, $"-APIV2 \"{apiV2Path}\"");
                        var dumpFullApi = Process.Start(studioPath, $"-FullAPI \"{fullApiPath}\"");

                        dumpApi.WaitForExit();
                        dumpApiV2.WaitForExit();
                        dumpFullApi.WaitForExit();
#endif
                    }

                    versionData.Version = versionId;
                    versionData.VersionGuid = buildVersion;
                    versionData.VersionOverload = targetVersion;

                    ProgressBarStyle = BarStyle.Marquee;
                }
                else
                {
                    ProgressBarStyle = BarStyle.Marquee;
                    echo("Update cancelled. Launching on current version.");
                }
            }
            else
            {
                echo("This version of Roblox Studio has been installed!");
            }

#if !CLI
            using (var http = new HttpClient())
            {
                OAuth2Config_JSON = await http.GetStringAsync(Program.BaseConfigUrl + "OAuth2Config.json");
                AppSettings_XML = await http.GetStringAsync(Program.BaseConfigUrl + "AppSettings.xml");
            }
            
            string appSettings = Path.Combine(studioDir, "AppSettings.xml");
            string oAuth2Config = Path.Combine(studioDir, "ApplicationConfig", "OAuth2Config.json");

            echo("Writing AppSettings.xml...");
            File.WriteAllText(appSettings, AppSettings_XML);

            echo("Writing OAuth2Config.json...");
            getDirectory(studioDir, "ApplicationConfig");
            File.WriteAllText(oAuth2Config, OAuth2Config_JSON);

            // Only update the registry protocols if the main registry
            // is the global one assigned to the program itself.

            if (mainState == Program.State && OperatingSystem.IsWindows())
            {
                setStatus("Configuring Roblox Studio...");
                echo("Updating registry protocols...");

                var studioPath = GetLocalStudioPath();
                Program.UpdateStudioRegistryProtocols(studioPath);
            }

            if (ApplyModManagerPatches && string.IsNullOrEmpty(OverrideStudioDirectory))
            {
                echo("Applying flag configuration...");
                FlagEditor.ApplyFlags();

                // Secret feature only for me :(
                // Feel free to patch in your own thing if you want.

                #if ROBLOX_INTERNAL
                    var rbxInternal = Task.Run(() => RobloxInternal.Patch(this));
                    await rbxInternal.ConfigureAwait(false);
                #endif
            }

            ProgressBarStyle = BarStyle.Marquee;
            MaxProgress = 1;
            Progress = 1;

            setStatus("Starting Roblox Studio...");
#else
            setStatus("Bootstrap complete!");
#endif
            echo("Roblox Studio is up to date!");

            await Task.Delay(1000);
            return true;
        }
    }
}
