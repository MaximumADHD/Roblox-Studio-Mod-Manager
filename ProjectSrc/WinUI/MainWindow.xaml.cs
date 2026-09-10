using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using RobloxDeployHistory;
using RobloxStudioModManager;

namespace RobloxStudioModManager.WinUI
{
    // The main Window class for the Mod Manager.
    public sealed partial class MainWindow : Window
    {
        private static VersionManifest versionRegistry => RobloxStudioModManager.Program.State.VersionData;
        private readonly string[] args;

        public MainWindow(string[] mainArgs = null)
        {
            InitializeComponent();
            Title = "Roblox Studio Mod Manager";

            if (mainArgs != null && mainArgs.Length > 0)
                args = mainArgs;

            ApplyBackdrop();
            configureWindow();

            ReleaseTagText.Text = RobloxStudioModManager.Program.ReleaseTag;

            Activated += MainWindow_Activated;
        }

        private void ApplyBackdrop()
        {
            SystemBackdrop = BackdropHelper.IsWindows11
                ? (SystemBackdrop)new MicaBackdrop()
                : new DesktopAcrylicBackdrop();
        }

        private void configureWindow()
        {
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "Icon.ico"));

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }

            WindowPositioning.ResizeAndCenter(AppWindow, 440, 416);
        }

        private void setBusy(bool busy)
        {
            RootGrid.IsHitTestVisible = !busy;
            LoadingRing.IsActive = busy;
        }

        private bool loaded;

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (loaded)
                return;

            loaded = true;
            await onLoaded();
        }

        private async Task onLoaded()
        {
            setBusy(true);

            if (args != null)
            {
                await launchStudio();
                return;
            }

            try
            {
                using (var http = new WebClient())
                {
                    string releaseTag = await http.DownloadStringTaskAsync(RobloxStudioModManager.Program.BaseConfigUrl + "LatestReleaseTag.txt");
                    releaseTag = releaseTag.Trim();

                    if (releaseTag != RobloxStudioModManager.Program.ReleaseTag)
                        await promptNewRelease(releaseTag);
                }
            }
            catch
            {
                await showError("Could not fetch latest release tag!\nYou may not have an internet connection.");
            }

            await setVersionHistory();
            setBusy(false);
        }

        private async Task promptNewRelease(string releaseTag)
        {
#if !DEBUG
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Update available!",

                Content = "There's a new version of the mod manager available!\n" +
                    "This version is likely broken or no longer supported.\n" +
                    "Would you like to check it out?",

                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowWithSoundAsync();

            if (result == ContentDialogResult.Primary)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://www.github.com/{RobloxStudioModManager.Program.RepoOwner}/{RobloxStudioModManager.Program.RepoName}/releases/tag/{releaseTag}",
                    UseShellExecute = true
                });

                Application.Current.Exit();
            }
#else
            await Task.CompletedTask;
#endif
        }

        private async Task showError(string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Error",
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowWithSoundAsync();
        }

        private async Task setVersionHistory(string channelName = "LIVE", string channelToken = "")
        {
            string targetId = RobloxStudioModManager.Program.State.TargetVersion;
            const string latest = "(Use Latest)";

            TargetVersionBox.Items.Clear();
            TargetVersionBox.Items.Add(latest);

            var deployLogs = await StudioDeployLogs.Get(RobloxStudioModManager.Program.AllowUnsupportedVersions, channelName, channelToken);
            HashSet<DeployLog> targets = deployLogs.CurrentLogs;

            TargetVersionBox.IsEnabled = deployLogs.HasHistory;
            TargetVersionLabel.Opacity = deployLogs.HasHistory ? 1.0 : 0.5;

            if (deployLogs.ChannelName == channelName || deployLogs.ChannelName == "zbeta")
            {
                if (!string.IsNullOrEmpty(channelName)) RobloxStudioModManager.Program.State.ChannelData.ChannelName = channelName;
                if (!string.IsNullOrEmpty(channelToken)) RobloxStudioModManager.Program.State.ChannelData.ChannelToken = channelToken;
            }
            else
            {
                await showError("Could not fetch client version info with the channel name and token!\nEnsure you have entered them correctly.");

                RobloxStudioModManager.Program.State.ChannelData.ChannelName = "LIVE";
                RobloxStudioModManager.Program.State.ChannelData.ChannelToken = "";

                ChannelNameBox.Text = "LIVE";
                ChannelTokenBox.Password = "";
            }

            if (deployLogs.HasHistory)
            {
                var items = targets.OrderByDescending(t => t.CommitId);

                foreach (var item in items)
                    TargetVersionBox.Items.Add(item);
            }

            DeployLog target = targets.FirstOrDefault(log => log.VersionId == targetId);

            if (target != null)
            {
                TargetVersionBox.SelectedItem = target;
                return;
            }

            TargetVersionBox.SelectedItem = latest;
        }

        public static string GetModPath()
        {
            string root = Path.Combine(RobloxStudioModManager.Program.RootDir, "ModFiles");

            if (!Directory.Exists(root))
            {
                // Build a folder structure so the usage is more clear.
                Directory.CreateDirectory(root);

                string[] folderPaths =
                {
                    "BuiltInPlugins",
                    "ClientSettings",

                    "content/avatar",
                    "content/fonts",
                    "content/models",
                    "content/scripts",
                    "content/sky",
                    "content/sounds",
                    "content/textures",
                    "content/translations"
                };

                foreach (string f in folderPaths)
                {
                    string path = Path.Combine(root, f);
                    Directory.CreateDirectory(path);
                }
            }

            return root;
        }

        private void ManageModsButton_Click(object sender, RoutedEventArgs e)
        {
            string modPath = GetModPath();

            Process.Start(new ProcessStartInfo
            {
                FileName = modPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private async Task<bool> showFlagWarningPrompt()
        {
            var dontShowAgain = new CheckBox { Content = "Do not show this warning again." };
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "WARNING: HERE BE DRAGONS",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close,

                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,

                            Text = "Editing flags can make Roblox Studio unstable, and could potentially corrupt your places and game data.\n\n" +
                                "You should not edit them unless you are just experimenting with new features locally, and you know what you're doing.\n\n" +
                                "Are you sure you would like to continue?"
                        },
                        dontShowAgain
                    }
                }
            };

            var result = await dialog.ShowWithSoundAsync();
            if (result == ContentDialogResult.Primary)
            {
                RobloxStudioModManager.Program.State.DisableFlagWarning = dontShowAgain.IsChecked == true;
                return true;
            }

            return false;
        }

        private async void EditFlagsButton_Click(object sender, RoutedEventArgs e)
        {
            bool allow = RobloxStudioModManager.Program.State.DisableFlagWarning;

            if (!allow)
                allow = await showFlagWarningPrompt();

            if (!allow)
                return;

            setBusy(true);

            var info = await StudioBootstrapper.GetCurrentVersionInfo();
            AppWindow.Hide();

            await BootstrapWindow.BringUpToDate(info.VersionGuid, "Some newer flags might be missing.", Content.XamlRoot);

            var editor = new FlagEditorWindow();
            var editorClosed = new TaskCompletionSource<bool>();
            editor.Closed += (s, args2) => editorClosed.TrySetResult(true);
            editor.Activate();
            await editorClosed.Task;

            AppWindow.Show();
            setBusy(false);
        }

        private async void LaunchStudioButton_Click(object sender, RoutedEventArgs e)
        {
            await launchStudio();
        }

        private async Task launchStudio()
        {
            var overrideGuid = "";

            if (args != null && args.Length > 0 && args[0].StartsWith("version-"))
                overrideGuid = args[0];

            var bootstrapper = new StudioBootstrapper
            {
                ForceInstall = ForceRebuildCheck.IsChecked == true,
                ApplyModManagerPatches = true,
                OverrideGuid = overrideGuid
            };

            AppWindow.Hide();

            var bootstrapWindow = new BootstrapWindow(bootstrapper);
            await bootstrapWindow.Bootstrap();
            bootstrapWindow.RequestClose();

            string studioRoot = StudioBootstrapper.GetStudioDirectory();
            string modPath = GetModPath();

            var modFiles = Directory.GetFiles(modPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in modFiles)
            {
                try
                {
                    var info = new FileInfo(file);
                    var filePath = file;
                    var delete = false;

                    if (info.Length == 0 && info.Name.StartsWith("DELETE"))
                    {
                        var dir = info.DirectoryName;

                        var realName = info.Name
                            .Substring(6)
                            .TrimStart();

                        filePath = Path.Combine(dir, realName);
                        delete = true;
                    }

                    string relativeFile = filePath.Replace(modPath, studioRoot);

                    string relativeDir = Directory
                        .GetParent(relativeFile)
                        .ToString();

                    if (!Directory.Exists(relativeDir))
                        Directory.CreateDirectory(relativeDir);

                    byte[] contents = info.Length > 0
                        ? File.ReadAllBytes(file)
                        : Array.Empty<byte>();

                    if (File.Exists(relativeFile))
                    {
                        byte[] relative = File.ReadAllBytes(relativeFile);

                        if (relative.Length == contents.Length)
                            if (relative.SequenceEqual(contents))
                                continue;

                        if (delete)
                        {
                            File.Delete(relativeFile);
                            continue;
                        }

                        info.CopyTo(relativeFile, true);
                    }
                    else
                    {
                        if (delete)
                            continue;

                        File.WriteAllBytes(relativeFile, contents);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to overwrite {0}!", file);
                }
            }

            var robloxStudioInfo = new ProcessStartInfo()
            {
                FileName = StudioBootstrapper.GetStudioPath(),
                Arguments = ""
            };

            if (args != null)
            {
                string firstArg = args[0];

                if (firstArg != null && firstArg.StartsWith("roblox-studio", RobloxStudioModManager.Program.StringFormat))
                {
                    // Arguments were passed by URI.
                    robloxStudioInfo.Arguments = firstArg;
                }
                else
                {
                    // Arguments were passed directly.
                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i];

                        if (arg.Contains(' '))
                            arg = $"\"{arg}\"";

                        robloxStudioInfo.Arguments += ' ' + arg;
                    }
                }
            }

            if (OpenStudioDirectoryCheck.IsChecked == true)
            {
                Process.Start(new ProcessStartInfo { FileName = studioRoot, UseShellExecute = true });
            }
            else
            {
                string currentVersion = versionRegistry.VersionGuid;
                versionRegistry.LastExecutedVersion = currentVersion;

                Process.Start(robloxStudioInfo);
            }

            RobloxStudioModManager.Program.SaveState();
            Environment.Exit(Environment.ExitCode);
        }

        private async void TargetVersionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetVersionBox.SelectedIndex == 0)
            {
                RobloxStudioModManager.Program.State.TargetVersion = "";
                return;
            }

            if (!(TargetVersionBox.SelectedItem is DeployLog target))
                return;

            RobloxStudioModManager.Program.State.TargetVersion = target.VersionId;

            if (target.Unsupported)
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = Content.XamlRoot,
                    Title = "Tread Lightly!",

                    Content = "This version of Roblox Studio is no longer supported!\n" +
                        "You are strongly advised against using it in the future.\n" +
                        "No technical support will be provided, use it at your own risk!",

                    CloseButtonText = "OK"
                };

                await dialog.ShowWithSoundAsync();
            }
        }

        // We have a debounce between ChannelTokenBox and ChannelNameBox,
        // to prevent the dropdown from having to constantly re-construct the items.
        private CancellationTokenSource versionHistoryDebounce;

        private async void ChannelTokenBox_LostFocus(object sender, RoutedEventArgs e)
        {
            versionHistoryDebounce?.Cancel();
            var cts = new CancellationTokenSource();
            versionHistoryDebounce = cts;

            try
            {
                await Task.Delay(400, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            await setVersionHistory(ChannelNameBox.Text, ChannelTokenBox.Password);
        }
    }
}
