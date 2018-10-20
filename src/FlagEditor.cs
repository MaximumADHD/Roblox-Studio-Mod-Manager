using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class FlagEditor : Form
    {
        private static RegistryKey flagRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FlagEditor");

        private DataTable flagTable;
        private Launcher launcher;
        private string branch;
        
        private static string[] fvarGroups = new string[3] { "F", "DF", "SF" };
        private static string[] fvarTypes = new string[4] { "Flag", "String", "Int", "Log" };

        private const string OVERRIDE_STATUS_OFF = "No local overrides were found on load.";
        private const string OVERRIDE_STATUS_ON = "Values highlighted red were overridden locally.";

        private static List<string> fvarPrefixes;

        static FlagEditor()
        {
            fvarPrefixes = new List<string>();

            foreach (string fvarType in fvarTypes)
            {
                foreach (string fvarGroup in fvarGroups)
                {
                    fvarPrefixes.Add(fvarGroup + fvarType);
                }
            }
        }

        public FlagEditor(Launcher _launcher, string _branch)
        {
            InitializeComponent();
            
            launcher = _launcher;
            launcher.Enabled = false;

            branch = _branch;
        }

        private bool confirm(string header, string message, bool closeIfNoSelected = false)
        {
            DialogResult result = MessageBox.Show(message, header, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                if (closeIfNoSelected)
                    Close();

                return false;
            }

            return true;
        }

        private static string getEnvironmentFile(string env, string relativePath)
        {
            string envPath = Environment.GetEnvironmentVariable(env);
            return Path.Combine(envPath, relativePath);
        }

        private static string findStudioAppSettings()
        {
            const string settingsPath = "Roblox/ClientSettings/StudioAppSettings.json";

            // Check if the file exists in Program Files (x86)
            string checkProgramFiles = getEnvironmentFile("ProgramFiles(x86)", settingsPath);
            if (File.Exists(checkProgramFiles))
                return checkProgramFiles;

            // Check if the file exists in AppData\Local
            string checkLocalAppData = getEnvironmentFile("LocalAppData", settingsPath);
            if (File.Exists(checkLocalAppData))
                return checkLocalAppData;

            return "";
        }

        private async void FlagEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            string studioPath = Path.Combine(RobloxInstaller.GetStudioDirectory(), "RobloxStudioBeta.exe");

            string currentVersion = Program.GetRegistryString(Program.ModManagerRegistry, "BuildVersion");
            string liveVersion = await RobloxInstaller.GetCurrentVersion(branch);

            if (currentVersion != liveVersion)
            {
                bool doInstall = confirm("Roblox Studio is out of date!", "The listed flags may not be accurate.\nWould you like to update now?");
                if (doInstall)
                {
                    RobloxInstaller installer = new RobloxInstaller(false);
                    studioPath = await installer.RunInstaller(branch);
                    installer.Dispose();

                    TopMost = true;
                    BringToFront();
                }
            }

            string settingsPath = findStudioAppSettings();
            string lastExecVersion = Program.GetRegistryString(Program.ModManagerRegistry, "LastExecutedVersion");

            if (lastExecVersion != liveVersion || settingsPath == "")
            {
                // Run Roblox Studio briefly so we can update the settings file.
                ProcessStartInfo studioStartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    FileName = studioPath,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process studio = Process.Start(studioStartInfo);
                DateTime startTime = DateTime.Now;

                // Wait for the settings path file to exist.
                while (settingsPath.Length == 0)
                {
                    settingsPath = findStudioAppSettings();
                    await Task.Delay(100);
                }

                // Wait for the file to be updated.
                FileInfo info = new FileInfo(settingsPath);

                while (info.LastWriteTime.Ticks < startTime.Ticks)
                {
                    await Task.Delay(100);
                    info.Refresh();
                }

                // Wait just a moment so we don't access the file while its in a write lock.
                await Task.Delay(500);

                // Should be good now. Nuke studio and flag the version we updated with.
                Program.ModManagerRegistry.SetValue("LastExecutedVersion", liveVersion);
                studio.Kill();
            }

            string settings = File.ReadAllText(settingsPath);

            settings = settings
                .Replace('\r', ' ').Replace('\n', ' ')
                .Replace('{',  ' ').Replace('}',  ' ');

            flagTable = new DataTable();
            flagTable.Columns.Add("Name");
            flagTable.Columns.Add("Type");
            flagTable.Columns.Add("Value");

            DataGridViewRowCollection rows = flagDataGridView.Rows;
            string[] kvPairs = settings.Split(',');

            flagDataGridView.SuspendLayout();

            foreach (string kvPairStr in kvPairs)
            {
                string[] kvPair = kvPairStr.Split(':');

                if (kvPair.Length == 2 )
                {
                    string key = kvPair[0].Replace('"', ' ').Trim();
                    string value = kvPair[1].Replace('"', ' ').Trim();

                    string type = fvarPrefixes
                        .Where(fvar => key.StartsWith(fvar))
                        .FirstOrDefault();

                    if (type.Length > 0)
                    {
                        string name = key.Substring(type.Length);
                        flagTable.Rows.Add(name, type, value);
                    }
                }
            }

            flagDataGridView.DataSource = flagTable;
            flagDataGridView.ResumeLayout();

            // Set the sort modes and widths of the columns.
            var columns = flagDataGridView.Columns;

            var nameColumn = columns[0];
            nameColumn.Width = 250;
            nameColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;

            var typeColumn = columns[1];
            typeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            typeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            var valueColumn = columns[2];
            valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            valueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Apply alphabetical sort to the name column.
            flagDataGridView.Sort(nameColumn, ListSortDirection.Ascending);

            Enabled = true;
            UseWaitCursor = false;
        }

        private void FlagEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            launcher.Enabled = true;
            launcher.BringToFront();
            launcher = null;
        }

        private void flagSearchFilter_TextChanged(object sender, EventArgs e)
        {
            string text = flagSearchFilter.Text;
            string filtered = new string(text.Where(char.IsLetterOrDigit).ToArray());

            if (text != filtered)
            {
                int selectionStart = flagSearchFilter.SelectionStart;
                flagSearchFilter.Text = filtered;
                flagSearchFilter.SelectionStart = selectionStart;
                return;
            }
            else
            {
                flagTable.DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", text);
                flagDataGridView.Sort(flagDataGridView.Columns[0], ListSortDirection.Ascending);
            }
        }

        private void flagDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void flagSearchFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode < Keys.A || e.KeyCode > Keys.Z) && e.KeyCode != Keys.Back)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void overrideSelected_Click(object sender, EventArgs e)
        {

        }
    }
}
