using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobloxStudioModManager
{
    public partial class BootstrapperForm : Form
    {
        public StudioBootstrapper Bootstrapper { get; private set; }
        private readonly bool exitOnClose = false;

        public BootstrapperForm(StudioBootstrapper bootstrapper, bool exitWhenClosed = false)
        {
            Contract.Requires(bootstrapper != null);
            InitializeComponent();

            Bootstrapper = bootstrapper;
            exitOnClose = exitWhenClosed;

            bootstrapper.EchoFeed += new MessageEventHandler(Bootstrapper_EchoFeed);
            bootstrapper.StatusChanged += new MessageEventHandler(Bootstrapper_StatusChanged);

            bootstrapper.ProgressChanged += new ChangeEventHandler<int>(Bootstrapper_ProgressChanged);
            bootstrapper.ProgressBarStyleChanged += new ChangeEventHandler<ProgressBarStyle>(Bootstrapper_ProgressBarStyleChanged);

            Show();
            BringToFront();
        }

        public async Task Bootstrap()
        {
            string targetVersion = Program.GetString("TargetVersion");
            var bootstrap = Bootstrapper.Bootstrap(targetVersion);

            await bootstrap.ConfigureAwait(true);
        }

        public static async Task BringUpToDate(string branch, string expectedVersion, string updateReason)
        {
            string currentVersion = Program.VersionRegistry.GetString("VersionGuid");

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
                    var bootstrapper = new StudioBootstrapper() { Branch = branch };

                    using (var installer = new BootstrapperForm(bootstrapper))
                    {
                        var bootstrap = installer.Bootstrap();
                        await bootstrap.ConfigureAwait(true);
                    }
                }
            }
        }

        private void UpdateStatusMetric()
        {
            string text = "";

            if (progressBar.Style == ProgressBarStyle.Continuous)
            {
                int progress = progressBar.Value;
                int progressMax = progressBar.Maximum;

                text = $"(Progress Metric: {progress}/{progressMax})";
            }

            if (progressMetric.Text == text)
                return;

            progressMetric.Text = text;
            progressMetric.Refresh();
        }

        private void Bootstrapper_StatusChanged(object sender, MessageEventArgs e)
        {
            if (statusLbl.InvokeRequired)
            {
                var inThread = new MessageEventHandler(Bootstrapper_StatusChanged);
                statusLbl.Invoke(inThread, sender, e);
            }
            else
            {
                statusLbl.Text = e.Message;
                statusLbl.Refresh();

                BringToFront();
                UpdateStatusMetric();
            }
        }

        private void Bootstrapper_EchoFeed(object sender, MessageEventArgs e)
        {
            if (log.InvokeRequired)
            {
                var inThread = new MessageEventHandler(Bootstrapper_EchoFeed);
                log.Invoke(inThread, sender, e);
            }
            else
            {
                if (!string.IsNullOrEmpty(log.Text))
                    log.AppendText("\n");

                log.AppendText(e.Message);
            }
        }

        private void Bootstrapper_ProgressChanged(object sender, ChangeEventArgs<int> e)
        {
            if (progressBar.InvokeRequired)
            {
                var inThread = new ChangeEventHandler<int>(Bootstrapper_ProgressChanged);
                progressBar.Invoke(inThread, sender, e);
            }
            else
            {
                int value = e.Value;
                var context = e.Context;

                switch (context)
                {
                    case "Progress":
                    {
                        progressBar.Value = Math.Min(value, progressBar.Maximum);
                        break;
                    }
                    case "MaxProgress":
                    {
                        if (progressBar.Value > value)
                            progressBar.Value = (value - 1);

                        progressBar.Maximum = value;
                        break;
                    }
                }

                UpdateStatusMetric();
            }
        }

        private void Bootstrapper_ProgressBarStyleChanged(object sender, ChangeEventArgs<ProgressBarStyle> e)
        {
            if (progressBar.InvokeRequired)
            {
                var inThread = new ChangeEventHandler<ProgressBarStyle>(Bootstrapper_ProgressBarStyleChanged);
                progressBar.Invoke(inThread, sender, e);
            }
            else
            {
                var style = e.Value;
                progressBar.Style = style;

                UpdateStatusMetric();
            }
        }

        private void BootstrapperForm_FormClosed(object sender, FormClosedEventArgs e)
        {
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
