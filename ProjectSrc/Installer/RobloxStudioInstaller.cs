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
    public partial class RobloxStudioInstaller : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        public string setupDir;
        public string buildVersion;
        public string robloxStudioBetaPath;

        public delegate void EchoDelegator(string text);
        public delegate void IncrementDelegator(int count);

        private int actualProgressBarSum = 0;
        private bool exitWhenClosed = true;

        private const string appSettingsXml =
            "<Settings>\n" +
            "   <ContentFolder>content</ContentFolder>\n" +
            "   <BaseUrl>http://www.roblox.com</BaseUrl>\n" +
            "</Settings>";

        private const string amazonAws = "https://s3.amazonaws.com/";

        private static RegistryKey pkgRegistry = Program.GetSubKey("PackageManifest");
        private static RegistryKey fileRegistry = Program.GetSubKey("FileManifest");
        private static RegistryKey fixedRegistry = Program.GetSubKey(fileRegistry, "Fixed");

        private static WebClient http = new WebClient();

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
                EchoDelegator echoer = new EchoDelegator(echo);
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
                IncrementDelegator increment = new IncrementDelegator(incrementProgressBarMax);
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
                IncrementDelegator increment = new IncrementDelegator(progressBar.Increment);
                progressBar.Invoke(increment, count);
            }
            else
            {
                progressBar.Increment(count);
            }
        }

        private string constructDownloadUrl(string file)
        {
            return amazonAws + setupDir + buildVersion + '-' + file;
        }

        public static async Task BringUpToDate(string branch, string expectedVersion, string updateReason)
        {
            string currentVersion = Program.GetRegistryString("BuildVersion");

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
                    RobloxStudioInstaller installer = new RobloxStudioInstaller(false);

                    await installer.RunInstaller(branch);
                    installer.Dispose();
                }
            }
        }

        public RobloxStudioInstaller(bool _exitWhenClosed = true)
        {
            InitializeComponent();
            http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");
            exitWhenClosed = _exitWhenClosed;
        }

        private static string getDirectory(params string[] paths)
        {
            string basePath = Path.Combine(paths);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return basePath;
        }

        public static string GetStudioBinaryType()
        {
            string binaryType = "WindowsStudio";

            if (Program.GetRegistryString("BuildType") == "64-bit")
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
                string fastUrl = $"{amazonAws}/setup.{branch}.com/versionQTStudio";
                return await http.DownloadStringTaskAsync(fastUrl);
            }
        }

        public static async Task<ClientVersionInfo> GetCurrentVersionInfo(string branch)
        {
            string binaryType = GetStudioBinaryType();

            if (branch == "roblox")
            {
                var info = await ClientVersionInfo.Get(binaryType);
                return info;
            }
            else
            {
                // Unfortunately as of right now, the ClientVersionInfo end-point on 
                // gametest isn't available to the public, so I have to use some hacks
                // with the DeployHistory.txt file to figure out what version guid to use.

                var logData = await StudioDeployLogs.Get(branch);

                var currentLogs = logData.CurrentLogs;
                int numLogs = currentLogs.Count;

                DeployLog latest = currentLogs[numLogs - 1];
                DeployLog prev = currentLogs[numLogs - 2];

                DeployLog build_x86, build_x64;

                // If these builds aren't using the same perforce changelist,
                // then the 64-bit version hasn't been deployed yet. There is
                // usually a ~5 minute gap between the new 32-bit version being
                // deployed, and the 64-bit version proceeding it.

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
                
                return info;
            }
        }

        // YOU WERE SO CLOSE ROBLOX, AGHHHH
        private static string fixFilePath(string pkgName, string filePath)
        {
            string pkgDir = pkgName.Replace(".zip", "");

            if ((pkgDir == "Plugins" || pkgDir == "Qml") && !filePath.StartsWith(pkgDir))
                filePath = pkgDir + '\\' + filePath;

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

                echo($"Writing {filePath}...");
                entry.ExtractToFile(extractPath);

                fileRegistry.SetValue(filePath, newFileSig);
            }
            catch
            {
                echo($"FILE WRITE FAILED: {filePath} (This build may not run as expected)");
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

        public async Task<string> RunInstaller(string branch, bool forceInstall = false)
        {
            string rootDir = GetStudioDirectory();
            string downloads = getDirectory(rootDir, "downloads");

            Show();
            BringToFront();
            TopMost = true;

            setupDir = $"setup.{branch}.com/";
            robloxStudioBetaPath = GetStudioPath();

            setStatus("Checking for updates");
            echo("Checking build installation...");

            string currentBranch = Program.GetRegistryString("BuildBranch");
            string currentVersion = Program.GetRegistryString("BuildVersion");

            bool shouldInstall = (forceInstall || currentBranch != branch);
            string fastVersion = await GetFastVersionGuid(currentBranch);

            if (branch == "roblox")
                buildVersion = fastVersion;

            ClientVersionInfo versionInfo = null;

            if (shouldInstall || fastVersion != currentVersion)
            {
                if (currentBranch != "roblox")
                    echo("Possible update detected, verifying...");

                versionInfo = await GetCurrentVersionInfo(branch);
                buildVersion = versionInfo.Guid;
            }
            else
            {
                buildVersion = fastVersion;
            }

            if (currentVersion != buildVersion || shouldInstall)
            {
                echo("This build needs to be installed!");

                string binaryType = GetStudioBinaryType();
                string versionId = versionInfo.Version;

                bool safeToContinue = false;
                bool cancelled = false;

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

                        Show();
                        BringToFront();
                        
                        List<Process> runningNow = null;
                        const int retries = 10;

                        progressBar.Value = 0;
                        progressBar.Maximum = retries * 300;

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
                                progressBar.Increment(300);
                                await Task.Delay(1000);
                            }
                        }

                        if (runningNow.Count > 0 && !safeToContinue)
                        {
                            TopMost = true;
                            BringToFront();

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

                if (!cancelled)
                {
                    TopMost = true;
                    BringToFront();

                    setStatus($"Installing Version {versionId} of Roblox Studio...");
                    List<Task> taskQueue = new List<Task>();

                    echo("Grabbing package manifest...");
                    List<RobloxPackageManifest> pkgManifest = await RobloxPackageManifest.Get(branch, buildVersion);

                    echo("Grabbing file manifest...");
                    RobloxFileManifest fileManifest = await RobloxFileManifest.Get(branch, buildVersion);

                    progressBar.Maximum = 1;
                    progressBar.Value = 0;

                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Refresh();
                    
                    foreach (RobloxPackageManifest package in pkgManifest)
                    {
                        int size = package.Size;

                        string pkgName = package.Name;
                        string oldSig = pkgRegistry.GetValue(pkgName, "") as string;
                        string newSig = package.Signature;

                        if (oldSig == newSig && !forceInstall)
                        {
                            echo($"Package '{pkgName}' hasn't changed between builds, skipping.");
                            continue;
                        }

                        progressBar.Maximum += size;

                        Task installer = Task.Run( async() =>
                        {
                            string zipFileUrl = constructDownloadUrl(package.Name);
                            string zipExtractPath = Path.Combine(downloads, package.Name);

                            echo($"Installing package {zipFileUrl}");

                            WebClient localHttp = new WebClient();
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

                            // Files that we ran into that aren't in the manifest, which we couldn't extract
                            // immediately because the root directory had not been determined.
                            var postProcess = new Dictionary<ZipArchiveEntry, string>();

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

                                        // If we can't find this file in the signature lookup table,
                                        // try appending the local directory to it. This resolves some
                                        // edge cases relating to the fixFilePath function above.

                                        if (!fileManifest.FileToSignature.ContainsKey(filePath))
                                            filePath = localRootDir + filePath;

                                        // If we can find this file path in the file manifest, then we will
                                        // use its pre-computed signature to check if the file has changed.

                                        if (fileManifest.FileToSignature.ContainsKey(filePath))
                                        {
                                            newFileSig = fileManifest.FileToSignature[filePath];
                                        }
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
                                            writePackageFile(rootDir, pkgName, file, newFileSig, entry, forceInstall);

                                            // If we haven't resolved the directory being used by this package, it is
                                            // possible to infer what it is by comparing the local path in the zip file
                                            // to the path corresponding with this file signature.

                                            if (localRootDir == null)
                                            {
                                                string filePath = fixFilePath(pkgName, file);
                                                string entryPath = entry.FullName.Replace('/', '\\');

                                                // If our local path is the end of the file path in the package signature...
                                                if (filePath.EndsWith(entryPath))
                                                {
                                                    // We can infer what the root extraction directory is for the
                                                    // files in this package!                                            
                                                    localRootDir = filePath.Replace(entryPath, "");

                                                    /*  ===== MORE DETAILS ON THIS INFERENCE =====
                                                     *  
                                                     *  Lets say I am extracting the file: "textures\studs.dds"
                                                     *  From the zip file package:         "content-textures3.zip"
                                                     *  
                                                     *  We have the hash signature for each file and where it should be extracted to,
                                                     *  but do we need to compute and match the hashes for every single file? Is there
                                                     *  a way we could do it faster?
                                                     *  
                                                     *  Well, if we compute the hash for the "studs.dds" file, we might get something like:
                                                     *  77e6efcbc2129448a094dd7afa36e484
                                                     *  
                                                     *  We can then check this hash against the SignatureToFiles 
                                                     *  lookup table, which is derived from rbxManifest.txt:
                                                     *  
                                                     *  +----------------------------------+---------------------------------------+
                                                     *  |          File Signature          |               File Path               |
                                                     *  +----------------------------------+---------------------------------------+
                                                     *  | 77e6efcbc2129448a094dd7afa36e484 | PlatformContent\pc\textures\studs.dds |
                                                     *  +----------------------------------+---------------------------------------+
                                                     *  
                                                     *  By comparing the file path in the lookup table with our local path:
                                                     *  
                                                     *      PlatformContent\pc\textures\studs.dds
                                                     *                         textures\studs.dds
                                                     *                         
                                                     *  We can now infer that "PlatformContent\pc" is the local root directory that 
                                                     *  all files in "content-textures3.zip" will be extracted into, without needing
                                                     *  to compute the hash for any other files in the zip file package!
                                                     */
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
                                            postProcess.Add(entry, newFileSig);
                                        }
                                        else
                                        {
                                            // Append the local root directory.
                                            file = localRootDir + file;
                                            writePackageFile(rootDir, pkgName, file, newFileSig, entry, forceInstall);
                                        }
                                    }
                                }
                            }

                            // Process any files that we deferred from writing immediately.
                            // See the comment above the postProcess definition.
                            foreach (ZipArchiveEntry entry in postProcess.Keys)
                            {
                                string file = entry.FullName;
                                string newFileSig = postProcess[entry];

                                if (localRootDir != null)
                                    file = localRootDir + file;

                                writePackageFile(rootDir, pkgName, file, newFileSig, entry, forceInstall);
                            }

                            // Update the signature in the package registry so we can check if this zip file
                            // needs to be updated in future versions.

                            pkgRegistry.SetValue(pkgName, package.Signature);
                        });

                        taskQueue.Add(installer);
                    }

                    await Task.WhenAll(taskQueue.ToArray());

                    echo("Writing AppSettings.xml...");

                    string appSettings = Path.Combine(rootDir, "AppSettings.xml");
                    File.WriteAllText(appSettings, appSettingsXml);

                    setStatus("Deleting unused files...");
                    taskQueue.Clear();

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

                        string filePath = Path.Combine(rootDir, fileName);

                        if (!File.Exists(filePath))
                        {
                            // Check if we recorded this file as an error in the manifest.
                            string fixedFile = Program.GetRegistryString(fixedRegistry, fileName);
                            if (fixedFile.Length > 0)
                            {
                                // Use this path instead.
                                filePath = Path.Combine(rootDir, fixedFile);
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
                                            // Record it for future reference so we don't have to waste time
                                            // computing the signature.

                                            string fixedName = fileManifest.SignatureToFiles[signature].First();
                                            fixedRegistry.SetValue(fileName, fixedName);
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

                    await Task.WhenAll(taskQueue.ToArray());

                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.Refresh();

                    Program.ModManagerRegistry.SetValue("BuildBranch", branch);
                    Program.ModManagerRegistry.SetValue("BuildVersion", buildVersion);
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

            Program.UpdateStudioRegistryProtocols(setupDir, buildVersion, robloxStudioBetaPath);

            if (exitWhenClosed)
            {
                echo("Applying flag configuration...");
                FlagEditor.ApplyFlags();

                echo("Patching explorer icons...");
                await ExplorerIconEditor.PatchExplorerIcons();
            }
            
            setStatus("Starting Roblox Studio...");
            echo("Roblox Studio is up to date!");

            await Task.Delay(1000);
            return robloxStudioBetaPath;
        }

        private void RobloxStudioInstaller_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (exitWhenClosed)
            {
                Application.Exit();
            }
        }
    }
}
