using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

using RobloxStudioModManager;

namespace RobloxStudioModManager.WinUI
{
    public sealed partial class BootstrapWindow : Window
    {
        public StudioBootstrapper Bootstrapper { get; }

        private ConcurrentBag<string> logQueue = new ConcurrentBag<string>();
        private readonly ObservableCollection<string> logLines = new ObservableCollection<string>();
        private readonly bool exitOnClose;
        private DispatcherTimer progressTimer;
        private bool programmaticClose;

        // Initializes the main bootstrap window.
        public BootstrapWindow(StudioBootstrapper bootstrapper, bool exitWhenClosed = false)
        {
            Contract.Requires(bootstrapper != null);
            InitializeComponent();

            Bootstrapper = bootstrapper;
            exitOnClose = exitWhenClosed;

            Title = "Roblox Studio Bootstrapper";
            ApplyBackdrop();
            configureWindow();

            LogListView.ItemsSource = logLines;

            bootstrapper.EchoFeed += new MessageFeed(echo);
            bootstrapper.StatusFeed += new MessageFeed(setStatus);
            bootstrapper.ConfirmFeed += new ConfirmPrompt(confirm);

            AppWindow.Closing += appWindow_Closing;
            Closed += bootstrapWindow_Closed;

            Activate();
        }

        // Applies a pretty backdrop to the background.
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
                presenter.IsMinimizable = true;
                presenter.IsAlwaysOnTop = true;
            }

            WindowPositioning.ResizeAndCenter(AppWindow, 560, 340);
        }

        public async Task Bootstrap()
        {
            var state = RobloxStudioModManager.Program.State;
            var targetVersion = state.TargetVersion;

            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            progressTimer.Tick += (sender, args) =>
            {
                var progress = Bootstrapper.Progress;
                Progress.IsIndeterminate = Bootstrapper.ProgressBarStyle == BootstrapProgressStyle.Indeterminate;

                var maxProgress = Bootstrapper.MaxProgress;
                Progress.Maximum = maxProgress;

                if (progress > maxProgress)
                {
                    Progress.Value = Math.Max(0, maxProgress - 1);
                    return;
                }

                if (!logQueue.IsEmpty)
                {
                    var newQueue = new ConcurrentBag<string>();
                    var drained = Interlocked.Exchange(ref logQueue, newQueue);

                    foreach (var line in drained)
                        logLines.Add(line);

                    LogListView.ScrollIntoView(logLines[logLines.Count - 1]);
                }

                Progress.Value = Math.Min(progress, maxProgress);
            };

            progressTimer.Start();

            var bootstrap = Bootstrapper.Bootstrap(targetVersion);
            await bootstrap.ConfigureAwait(true);
        }

        // Closes the window without triggering the "installation hasn't
        // finished" confirmation prompt.
        public void RequestClose()
        {
            programmaticClose = true;
            Close();
        }

        public static async Task BringUpToDate(string expectedVersion, string updateReason, XamlRoot xamlRoot)
        {
            var versionData = RobloxStudioModManager.Program.State.VersionData;
            string currentVersion = versionData.VersionGuid;

            if (currentVersion != expectedVersion)
            {
                bool proceed = true;

                if (!string.IsNullOrEmpty(currentVersion))
                {
                    var dialog = new ContentDialog
                    {
                        XamlRoot = xamlRoot,
                        Title = "Out of date!",

                        Content = "Roblox Studio is out of date!\n"
                            + updateReason +
                            "\nWould you like to update now?",

                        PrimaryButtonText = "Yes",
                        CloseButtonText = "No",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    var result = await dialog.ShowWithSoundAsync();
                    proceed = result == ContentDialogResult.Primary;
                }

                if (proceed)
                {
                    var bootstrapper = new StudioBootstrapper();
                    var installer = new BootstrapWindow(bootstrapper);

                    await installer.Bootstrap();
                    installer.RequestClose();
                }
            }
        }

        private void setStatus(string status)
        {
            if (DispatcherQueue.HasThreadAccess)
                StatusText.Text = status;
            else
                DispatcherQueue.TryEnqueue(() => StatusText.Text = status);
        }

        private void echo(string msg)
        {
            logQueue.Add(msg);
        }

        // Backs StudioBootstrapper.ConfirmFeed. ContentDialog.ShowAsync() must
        // run on this window's UI thread, so this always marshals through
        // DispatcherQueue rather than assuming the caller is already there.
        private Task<bool> confirm(string message, string title)
        {
            var tcs = new TaskCompletionSource<bool>();

            bool enqueued = DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var dialog = new ContentDialog
                    {
                        XamlRoot = Content.XamlRoot,
                        Title = title,
                        Content = message,
                        PrimaryButtonText = "OK",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary
                    };

                    var result = await dialog.ShowWithSoundAsync();
                    tcs.SetResult(result == ContentDialogResult.Primary);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!enqueued)
                tcs.SetResult(true);

            return tcs.Task;
        }

        private async void appWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (programmaticClose)
                return;

            // WinUI doesn't have CloseReason like WinForms,
            // so we pause the close, ask and then either let it through or just cancel it.
            args.Cancel = true;

            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Warning",

                Content = "The installation has not finished yet!\n" +
                    "Closing this window will exit the mod manager.\n" +
                    "Are you sure you want to continue?",

                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Close
            };

            var result = await dialog.ShowWithSoundAsync();

            if (result == ContentDialogResult.Primary)
                Environment.Exit(0);
        }

        private void bootstrapWindow_Closed(object sender, WindowEventArgs args)
        {
            progressTimer?.Stop();
            RobloxStudioModManager.Program.SaveState();

            if (exitOnClose)
                Environment.Exit(0);
        }
    }
}
