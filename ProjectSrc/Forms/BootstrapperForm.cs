using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using RobloxDeployHistory;

namespace RobloxStudioModManager
{
    public partial class BootstrapperForm : Form
    {
        public StudioBootstrapper Bootstrapper { get; private set; }
        private ConcurrentBag<string> logQueue = new ConcurrentBag<string>();
        private readonly bool exitOnClose = false;

        public BootstrapperForm(StudioBootstrapper bootstrapper, bool exitWhenClosed = false)
        {
            Contract.Requires(bootstrapper != null);
            InitializeComponent();

            Bootstrapper = bootstrapper;
            exitOnClose = exitWhenClosed;

            bootstrapper.EchoFeed += new MessageFeed(echo);
            bootstrapper.StatusFeed += new MessageFeed(setStatus);

            Show();
            BringToFront();
        }

        public async Task Bootstrap()
        {
            var state = Program.State;
            var targetVersion = state.TargetVersion;

            progressTimer.Tick += new EventHandler((sender, args) =>
            {
                var progress = Bootstrapper.Progress;
                progressBar.Style = Bootstrapper.ProgressBarStyle;

                var maxProgress = Bootstrapper.MaxProgress;
                progressBar.Maximum = maxProgress;

                if (progress > maxProgress)
                {
                    progressBar.Value = Math.Max(0, maxProgress - 1);
                    return;
                }

                if (!logQueue.IsEmpty)
                {
                    string blob = string.Join("\n", logQueue) + '\n';
                    var newQueue = new ConcurrentBag<string>();

                    Interlocked.Exchange(ref logQueue, newQueue);
                    log.AppendText(blob);
                }

                progressBar.Value = Math.Min(progress, maxProgress);
                Refresh();
            });

            var bootstrap = Bootstrapper.Bootstrap(targetVersion);
            await bootstrap.ConfigureAwait(true);
        }

        public static async Task BringUpToDate(Channel channel, string expectedVersion, string updateReason)
        {
            var versionData = Program.State.VersionData;
            string currentVersion = versionData.VersionGuid;

            if (currentVersion != expectedVersion)
            {
                DialogResult check = DialogResult.Yes;

                if (!string.IsNullOrEmpty(currentVersion))
                {
                    check = MessageBox.Show
                    (
                        "Roblox Studio is out of date!\n"
                        + updateReason +
                        "\nWould you like to update now?",

                        "Out of date!",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );
                }
                
                if (check == DialogResult.Yes)
                {
                    var bootstrapper = new StudioBootstrapper() { Channel = channel };

                    using (var installer = new BootstrapperForm(bootstrapper))
                    {
                        var bootstrap = installer.Bootstrap();
                        await bootstrap.ConfigureAwait(true);
                    }
                }
            }
        }

        private void setStatus(string status)
        {
            if (statusLbl.InvokeRequired)
            {
                var action = new Action<string>(setStatus);
                statusLbl.Invoke(action, status);
            }
            else
            {
                statusLbl.Text = status;
                statusLbl.Refresh();
                BringToFront();
            }
        }

        private void echo(string msg)
        {
            logQueue.Add(msg);
        }

        private void BootstrapperForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.SaveState();

            if (exitOnClose && e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }

        private void BootstrapperForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
                return;

            DialogResult result = MessageBox.Show
            (
                this,

                "The installation has not finished yet!\n" +
                "Closing this window will exit the mod manager.\n" +
                "Are you sure you want to continue?",

                "Warning",

                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.No)
            {
                Environment.Exit(0);
                return;
            }
            
            e.Cancel = true;
        }
    }
}
