using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

        private static string[] fvarGroups = new string[3] { "F", "DF", "SF" };
        private static string[] fvarTypes = new string[4] { "Flag", "String", "Int", "Log" };

        private static List<string> fvarPrefixes = new List<string>();

        private DataTable flagTable;
        private DataTable overrideTable;

        private Launcher launcher;
        private string branch;
        
        private Dictionary<string, DataGridViewRow> flagRowLookup = new Dictionary<string, DataGridViewRow>();
        private Dictionary<string, DataRow> overrideRowLookup = new Dictionary<string, DataRow>();

        private const string OVERRIDE_STATUS_OFF = "No local overrides were found on load.";
        private const string OVERRIDE_STATUS_ON = "Values highlighted in red were overridden locally.";

        private string updateFlag = "";

        static FlagEditor()
        {
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

            overrideStatus.Text = OVERRIDE_STATUS_OFF;
            overrideStatus.Visible = false;

            overrideStatus.Refresh();
        }

        private bool confirm(string header, string message)
        {
            DialogResult result = MessageBox.Show(message, header, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return (result == DialogResult.Yes);
        }

        private static void applyRowColor(DataGridViewRow row, Color color)
        {
            var cells = row.Cells;
            for (int i = 0; i < cells.Count; i++)
                cells[i].Style.BackColor = color;
        }
        
        private static string getFlagNameByRow(DataGridViewRow row)
        {
            var cells = row.Cells;

            string name = cells[0].Value as string;
            string type = cells[1].Value as string;

            return type + name;
        }

        private void refreshViewFlagRow(DataGridViewRow row)
        {
            string flagName = getFlagNameByRow(row);
            string[] flagNames = flagRegistry.GetSubKeyNames();

            if (flagNames.Contains(flagName))
            {
                RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                var valueCell = row.Cells[2];
                valueCell.Value = flagKey.GetValue("Value");

                applyRowColor(row, Color.Pink);
            }
        }

        private void markFlagEditorAsDirty()
        {
            updateFlag = DateTime.Now.Ticks.ToString();
        }

        private static DataTable initializeDataGridView(DataGridView dgv)
        {
            DataTable table = new DataTable();

            // Initialize the columns.
            table.Columns.Add("Name" );
            table.Columns.Add("Type" );
            table.Columns.Add("Value");

            // Disable the grid view layout, and apply the table as the data source.
            dgv.SuspendLayout();
            dgv.DataSource = table;

            // Set the sort modes and widths of the columns.
            var columns = dgv.Columns;

            var nameColumn = columns[0];
            nameColumn.Width = 250;
            nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            var typeColumn = columns[1];
            typeColumn.Width = 50;
            typeColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            var valueColumn = columns[2];
            valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            valueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Apply alphabetical sort to the name column.
            dgv.Sort(nameColumn, ListSortDirection.Ascending);
            dgv.ClearSelection();

            return table;
        }

        private void addFlagOverride(string name, string type, string value, bool initializing = false)
        {
            string[] names = flagRegistry.GetSubKeyNames();
            string key = type + name;

            if (!overrideRowLookup.ContainsKey(key))
            {
                RegistryKey flagKey = null;
                
                if (names.Contains(key))
                {
                    flagKey = flagRegistry.OpenSubKey(key);
                }
                else
                {
                    flagKey = flagRegistry.CreateSubKey(key);
                    flagKey.SetValue("Name", name);
                    flagKey.SetValue("Type", type);
                    flagKey.SetValue("Value", value);
                    flagKey.SetValue("Reset", value);
                }
                
                if (flagRowLookup.ContainsKey(key))
                {
                    DataGridViewRow viewFlagRow = flagRowLookup[key];
                    refreshViewFlagRow(viewFlagRow);
                }

                DataRow row = overrideTable.Rows.Add(name, type, value);
                overrideRowLookup.Add(key, row);
            }

            overrideStatus.Text = OVERRIDE_STATUS_ON;
            overrideStatus.ForeColor = Color.Red;

            if (!initializing)
            {
                DataRow dataRow = overrideRowLookup[key];
                int index = overrideTable.Rows.IndexOf(dataRow);

                DataGridViewRow overrideRow = overrideDataGridView.Rows[index];
                overrideDataGridView.CurrentCell = overrideRow.Cells[0];
                tabs.SelectedTab = overridesTab;
            }
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
                bool doInstall = confirm("Out of date!", "Roblox Studio is out of date!\nThe listed flags may not be accurate.\nWould you like to update now?");
                if (doInstall)
                {
                    RobloxInstaller installer = new RobloxInstaller(false);
                    installer.ShowDialog();

                    studioPath = await installer.RunInstaller(branch);
                    installer.Dispose();

                    TopMost = true;
                    BringToFront();
                }
            }

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string settingsPath = Path.Combine(localAppData, "Roblox", "ClientSettings", "StudioAppSettings.json");
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

                Process  studio = Process.Start(studioStartInfo);
                DateTime startTime = DateTime.Now;
                FileInfo info = new FileInfo(settingsPath);

                // Wait for the settings path file to exist or for it to be updated.
                while (!info.Exists || info.LastWriteTime.Ticks < startTime.Ticks)
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

            string[] flagNames = flagRegistry.GetSubKeyNames();
            string settings = File.ReadAllText(settingsPath)
                .Replace('\r', ' ').Replace('\n', ' ')
                .Replace('{',  ' ').Replace('}',  ' ');
            
            // Initialize Flag Table
            flagTable = initializeDataGridView(flagDataGridView);

            foreach (string kvPairStr in settings.Split(','))
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

                        if (flagNames.Contains(name))
                        {
                            // Update what the flag should be reset to if removed?
                            RegistryKey flagKey = flagRegistry.OpenSubKey(name);
                            flagKey.SetValue("Reset", value);
                        }
                    }
                }
            }

            // Setup flag->row lookup table.
            foreach (DataGridViewRow row in flagDataGridView.Rows)
            {
                var cells = row.Cells;

                string name  = cells[0].Value as string;
                string type  = cells[1].Value as string;
                string value = cells[2].Value as string;

                flagRowLookup.Add(type + name, row);
            }

            // Initialize Override Table
            overrideTable = initializeDataGridView(overrideDataGridView);
            
            foreach (string flagName in flagNames)
            {
                RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                string name  = Program.GetRegistryString(flagKey, "Name" );
                string type  = Program.GetRegistryString(flagKey, "Type" );
                string value = Program.GetRegistryString(flagKey, "Value");

                addFlagOverride(name, type, value, true);
            }

            var columns = overrideDataGridView.Columns;
            columns[0].ReadOnly = true;
            columns[1].ReadOnly = true;

            // Resume layout and enable interaction.
            flagDataGridView.CurrentCell = flagDataGridView[0, 0];
            flagDataGridView.ResumeLayout();

            overrideStatus.Visible = true;
            overrideDataGridView.ResumeLayout();

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
            }
            else
            {
                flagTable.DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", text);
                flagDataGridView.Sort(flagDataGridView.Columns[0], ListSortDirection.Ascending);
                markFlagEditorAsDirty();
            }
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
            if (flagDataGridView.SelectedRows.Count == 1)
            {
                var selectedRow = flagDataGridView.SelectedRows[0];
                var cells = selectedRow.Cells;

                string name  = cells[0].Value as string;
                string type  = cells[1].Value as string;
                string value = cells[2].Value as string;

                addFlagOverride(name, type, value);
            }
        }

        private void removeSelected_Click(object sender, EventArgs e)
        {
            var selectedRows = overrideDataGridView.SelectedRows;
            if (selectedRows.Count > 0)
            {
                var selectedRow = selectedRows[0];

                string flagName = getFlagNameByRow(selectedRow);
                RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                if (flagRowLookup.ContainsKey(flagName))
                {
                    var flagRow = flagRowLookup[flagName];
                    applyRowColor(flagRow, Color.White);

                    var valueCell = flagRow.Cells[2];
                    valueCell.Value = flagKey.GetValue("Reset");
                }

                if (overrideRowLookup.ContainsKey(flagName))
                {
                    DataRow rowToDelete = overrideRowLookup[flagName];
                    overrideRowLookup.Remove(flagName);
                    rowToDelete.Delete();
                }

                selectedRow.Visible = false;
                selectedRow.Dispose();

                flagRegistry.DeleteSubKey(flagName);
                markFlagEditorAsDirty();
            }

            if (overrideDataGridView.Rows.Count == 0)
            {
                overrideStatus.Text = OVERRIDE_STATUS_OFF;
                overrideStatus.ForeColor = Color.Black;
            }
        }

        private void removeAll_Click(object sender, EventArgs e)
        {
            bool doRemove = confirm("Confirmation", "Are you sure you would like to remove all flag overrides?");

            if (doRemove)
            {
                foreach (string flagName in flagRegistry.GetSubKeyNames())
                {
                    RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                    if (flagRowLookup.ContainsKey(flagName))
                    {
                        DataGridViewRow row = flagRowLookup[flagName];
                        applyRowColor(row, Color.White);

                        var valueCell = row.Cells[2];
                        valueCell.Value = flagKey.GetValue("Reset");
                    }

                    flagRegistry.DeleteSubKey(flagName);
                }

                overrideStatus.Text = OVERRIDE_STATUS_OFF;
                overrideStatus.ForeColor = Color.Black;

                overrideTable.Rows.Clear();
            }
        }

        private void overrideDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = overrideDataGridView.Rows[e.RowIndex];
            string flagName = getFlagNameByRow(row);

            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            string value = cell.Value as string;

            var cells = row.Cells;
            var flagType = cells[1].Value as string;

            // Check if this input should be cancelled.
            bool badInput = false;

            if (flagType.EndsWith("Flag"))
            {
                string test = value.ToLower().Trim();
                badInput = (test != "false" && test != "true");
            }
            else if (flagType.EndsWith("Int") || flagType.EndsWith("Log"))
            {
                int test = 0;
                badInput = !int.TryParse(value, out test);
            }

            RegistryKey flagKey = flagRegistry.OpenSubKey(flagName, true);

            if (!badInput)
            {
                if (flagRowLookup.ContainsKey(flagName))
                {
                    DataGridViewRow viewRow = flagRowLookup[flagName];
                    DataGridViewCell viewCell = viewRow.Cells[e.ColumnIndex];
                    viewCell.Value = cell.Value;
                }

                flagKey.SetValue("Value", value);
                markFlagEditorAsDirty();

                return;
            }

            // If we have bad input, reset the value to the original value.
            cell.Value = flagKey.GetValue("Reset");
        }

        private void overrideDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                DataGridViewRow row = overrideDataGridView.Rows[e.RowIndex];
                var cells = row.Cells;

                var valueCell = cells[2];
                Type cellType = valueCell.GetType();

                string flagType = cells[1].Value as string;
                if (flagType.EndsWith("Flag") && cellType != typeof(DataGridViewComboBoxCell))
                {
                    // Switch the cell to a combo box.
                    // The user needs to select either true or false.
                    var newValueCell = new DataGridViewComboBoxCell();
                    newValueCell.Items.Add("true");
                    newValueCell.Items.Add("false");
                    newValueCell.Value = valueCell.Value.ToString().ToLower();

                    row.Cells[2] = newValueCell;
                    newValueCell.ReadOnly = false;
                }
            }
        }

        private void overrideDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                DataGridViewRow row = overrideDataGridView.Rows[e.RowIndex];
                var cells = row.Cells;

                var valueCell = cells[2];
                Type cellType = valueCell.GetType();

                string flagType = cells[1].Value as string;
                if (flagType.EndsWith("Flag") && cellType != typeof(DataGridViewTextBoxCell))
                {
                    var newValueCell = new DataGridViewTextBoxCell();
                    newValueCell.Value = valueCell.Value.ToString();

                    row.Cells[2] = newValueCell;
                    newValueCell.ReadOnly = true;
                }
            }
        }

        // Whenever the updateFlag string is explicitly updated by the program, this will force
        // each cell to be redrawn. This will prevent any red-highlighted cells from being reset.
        private void flagDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = flagDataGridView.Rows[e.RowIndex];
            if (row.Tag == null || row.Tag.ToString() != updateFlag)
            {
                row.Tag = updateFlag;
                refreshViewFlagRow(row);
            }
        }
    }
}