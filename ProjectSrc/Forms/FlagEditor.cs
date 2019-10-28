using System;
using System.Collections.Generic;
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
        private static RegistryKey versionRegistry = Program.VersionRegistry;
        private static RegistryKey flagRegistry = Program.GetSubKey("FlagEditor");

        private string branch;

        private DataTable overrideTable;
        private Dictionary<string, DataRow> overrideRowLookup = new Dictionary<string, DataRow>();

        private const string OVERRIDE_STATUS_OFF = "No local overrides were found on load.";
        private const string OVERRIDE_STATUS_ON = "Values highlighted in red were overridden locally.";

        private List<FVariable> flags;
        private List<FVariable> allFlags;
        private Dictionary<string, int> flagLookup = new Dictionary<string, int>();

        private string currentSearch = "";
        private bool enterPressed = false;

        public FlagEditor(string _branch)
        {
            InitializeComponent();
            
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
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = color;
            }
        }
        
        private static string getFlagKeyByRow(DataGridViewRow row)
        {
            var cells = row.Cells;

            string name = cells[0].Value as string;
            string type = cells[1].Value as string;

            return type + name;
        }

        private bool rowMatchesSelectedRow(DataGridViewRow row)
        {
            if (flagDataGridView.SelectedRows.Count == 1)
            {
                DataGridViewRow selectedRow = flagDataGridView.SelectedRows[0];
                FVariable selectedFlag = flags[selectedRow.Index];

                return selectedFlag.Key == getFlagKeyByRow(row);
            }

            return false;
        }

        private void addFlagOverride(FVariable flag, bool init = false)
        {
            string key = flag.Key;

            if (!overrideRowLookup.ContainsKey(key))
            {
                RegistryKey editor = flagRegistry.GetSubKey(key);
                flag.SetEditor(editor);

                DataRow row = overrideTable.Rows.Add
                (
                    flag.Name, 
                    flag.Type, 
                    flag.Value
                );

                overrideRowLookup.Add(key, row);
            }

            overrideStatus.Text = OVERRIDE_STATUS_ON;
            overrideStatus.ForeColor = Color.Red;

            if (!init)
            {
                // Find the row that corresponds to the flag we added.
                var query = overrideDataGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(rowMatchesSelectedRow);

                if (query.Count() > 0)
                {
                    // Select it.
                    var overrideRow = query.First();
                    overrideDataGridView.CurrentCell = overrideRow.Cells[0];
                }
                
                // Switch to the overrides tab.
                tabs.SelectedTab = overridesTab;
            }
        }

        private void refreshFlags()
        {
            string search = flagSearchFilter.Text;

            flags = allFlags
                .Where(flag => flag.Name.Contains(search))
                .OrderBy(flag => flag.Name)
                .ToList();

            flagLookup.Clear();

            for (int i = 0; i < flags.Count; i++)
            {
                FVariable flag = flags[i];
                flagLookup[flag.Key] = i;
                flag.Dirty = true;
            }

            // Start populating flag browser rows.
            flagDataGridView.RowCount = flags.Count;
        }

        private async void initializeEditor()
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");

            string settingsDir = Path.Combine(localAppData, "Roblox", "ClientSettings");
            string settingsPath = Path.Combine(settingsDir, "StudioAppSettings.json");

            string lastExecVersion = versionRegistry.GetString("LastExecutedVersion");
            string versionGuid = versionRegistry.GetString("VersionGuid");

            if (lastExecVersion != versionGuid)
            {
                // Reset the settings file.
                Directory.CreateDirectory(settingsDir);
                File.WriteAllText(settingsPath, "");

                // Create some system events for studio so we can hide the splash screen.
                SystemEvent start = new SystemEvent("FFlagExtract");
                SystemEvent show = new SystemEvent("NoSplashScreen");

                // Run Roblox Studio briefly so we can update the settings file.
                ProcessStartInfo studioStartInfo = new ProcessStartInfo()
                {
                    FileName = StudioBootstrapper.GetStudioPath(),
                    Arguments = $"-startEvent {start.Name} -showEvent {show.Name}"
                };

                Process studio = Process.Start(studioStartInfo);
                await start.WaitForEvent();

                FileInfo info = new FileInfo(settingsPath);

                // Wait for the settings path to be written.
                while (info.Length == 0)
                {
                    await Task.Delay(30);
                    info.Refresh();
                }

                // Nuke studio and flag the version we updated with.
                versionRegistry.SetValue("LastExecutedVersion", versionGuid);
                studio.Kill();
            }

            // Initialize flag browser
            string[] flagNames = flagRegistry.GetSubKeyNames();

            string settings = File.ReadAllText(settingsPath);
            var json = Program.ReadJsonDictionary(settings);

            int numFlags = json.Count;
            var flagSetup = new List<FVariable>(numFlags);

            var autoComplete = new AutoCompleteStringCollection();
            
            foreach (var pair in json)
            {
                string key = pair.Key,
                       value = pair.Value;

                FVariable flag = new FVariable(key, value);
                autoComplete.Add(flag.Name);
                flagSetup.Add(flag);
                
                if (flagNames.Contains(flag.Name))
                {
                    // Update what the flag should be reset to if removed?
                    RegistryKey flagKey = flagRegistry.GetSubKey(flag.Name);
                    flagKey.SetValue("Reset", value);

                    // Set the flag's editor.
                    flag.SetEditor(flagKey);
                }
            }

            flagSearchFilter.AutoCompleteCustomSource = autoComplete;

            allFlags = flagSetup
                .OrderBy(flag => flag.Name)
                .ToList();

            refreshFlags();

            // Initialize override table.
            overrideTable = new DataTable();

            foreach (DataGridViewColumn column in overrideDataGridView.Columns)
                overrideTable.Columns.Add(column.DataPropertyName);

            DataView overrideView = new DataView(overrideTable);
            overrideView.Sort = "Name";

            foreach (string flagName in flagNames)
            {
                if (flagLookup.ContainsKey(flagName))
                {
                    int index = flagLookup[flagName];
                    FVariable flag = flags[index];
                    addFlagOverride(flag, true);
                }
            }
            
            overrideStatus.Visible = true;
            overrideDataGridView.DataSource = overrideView;
        }

        private async void FlagEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            TopMost = true;
            BringToFront();

            Refresh();

            await Task.Run(() =>
            {
                var initializer = new Action(initializeEditor);
                Invoke(initializer);
            });

            Enabled = true;
            UseWaitCursor = false;
        }

        private void flagDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            FVariable flag = flags[row];
            string value = "?";

            if (col == 0)
                value = flag.Name;
            else if (col == 1)
                value = flag.Type;
            else if (col == 2)
                value = flag.Value;

            e.Value = value;
        }

        private void flagSearchFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                enterPressed = true;
                e.Handled = true;
                ActiveControl = null;
            }
        }

        private void flagSearchFilter_Leave(object sender, EventArgs e)
        {
            if (enterPressed)
            {
                enterPressed = false;

                if (flagSearchFilter.Text != currentSearch)
                {
                    currentSearch = flagSearchFilter.Text;

                    Enabled = false;
                    Cursor = Cursors.WaitCursor;

                    flagDataGridView.RowCount = 0;
                    flagDataGridView.Refresh();

                    refreshFlags();

                    Enabled = true;
                    Cursor = Cursors.Default;
                }
            }
            else
            {
                flagSearchFilter.Text = currentSearch;
            }
        }

        private void overrideSelected_Click(object sender, EventArgs e)
        {
            if (flagDataGridView.SelectedRows.Count == 1)
            {
                var selectedRow = flagDataGridView.SelectedRows[0];
                FVariable selectedFlag = flags[selectedRow.Index];
                
                addFlagOverride(selectedFlag);
            }
        }

        private void removeSelected_Click(object sender, EventArgs e)
        {
            var selectedRows = overrideDataGridView.SelectedRows;

            if (selectedRows.Count > 0)
            {
                var selectedRow = selectedRows[0];
                string flagKey = getFlagKeyByRow(selectedRow);

                if (flagLookup.ContainsKey(flagKey))
                {
                    int index = flagLookup[flagKey];
                    FVariable flag = flags[index];
                    flag.ClearEditor();
                }
                
                if (overrideRowLookup.ContainsKey(flagKey))
                {
                    DataRow rowToDelete = overrideRowLookup[flagKey];
                    overrideRowLookup.Remove(flagKey);
                    rowToDelete.Delete();
                }

                selectedRow.Visible = false;
                selectedRow.Dispose();

                flagRegistry.DeleteSubKey(flagKey);
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
                    if (flagLookup.ContainsKey(flagName))
                    {
                        int index = flagLookup[flagName];
                        FVariable flag = flags[index];
                        flag.ClearEditor();
                    }

                    flagRegistry.DeleteSubKey(flagName);
                }

                overrideStatus.Text = OVERRIDE_STATUS_OFF;
                overrideStatus.ForeColor = Color.Black;

                overrideTable.Rows.Clear();
                overrideRowLookup.Clear();
            }
        }

        private void overrideDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = overrideDataGridView.Rows[e.RowIndex];
            string flagKey = getFlagKeyByRow(row);

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

            if (flagLookup.ContainsKey(flagKey))
            {
                int index = flagLookup[flagKey];
                FVariable flag = flags[index];

                if (!badInput)
                {
                    flag.SetValue(value);
                    return;
                }

                // If we have bad input, reset the value to the original value.
                cell.Value = flag.Reset;
            }
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

                    newValueCell.Value = valueCell.Value
                        .ToString()
                        .ToLower();

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

        private void flagDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int index = e.RowIndex;
            FVariable flag = flags[index];
            
            if (flag.Dirty)
            {
                var row = flagDataGridView.Rows[index];

                if (flag.Editor != null)
                {
                    var valueCell = row.Cells[2];
                    valueCell.Value = flag.Value;
                    applyRowColor(row, Color.Pink);
                }
                else
                {
                    applyRowColor(row, Color.White);
                }

                flag.Dirty = false;
            }
        }

        public static bool ApplyFlags()
        {
            try
            {
                List<string> configs = new List<string>();

                foreach (string flagName in flagRegistry.GetSubKeyNames())
                {
                    RegistryKey flagKey = flagRegistry.OpenSubKey(flagName);

                    string type = flagKey.GetString("Type"),
                           value = flagKey.GetString("Value");
                    
                    if (type.EndsWith("String"))
                        value = $"\"{value.Replace("\"", "")}\"";

                    configs.Add($"\t\"{flagName}\": {value}");
                };

                string json = "{\r\n" + string.Join(",\r\n", configs) + "\r\n}";
                string studioDir = StudioBootstrapper.GetStudioDirectory();

                string clientSettings = Path.Combine(studioDir, "ClientSettings");
                Directory.CreateDirectory(clientSettings);

                string filePath = Path.Combine(clientSettings, "ClientAppSettings.json");
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                Console.WriteLine("Failed to apply flag editor configuration!");
                return false;
            }
        }
    }
}