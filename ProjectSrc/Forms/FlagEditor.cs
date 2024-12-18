using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RobloxStudioModManager
{
    public partial class FlagEditor : Form
    {
        private static VersionManifest versionRegistry => Program.State.VersionData;
        private static SortedDictionary<string, FVariable> flagRegistry => Program.State.FlagEditor;

        private DataTable overrideTable;
        private readonly Dictionary<string, DataRow> overrideRowLookup = new Dictionary<string, DataRow>();

        private static readonly IReadOnlyDictionary<string, string> flagTypes = new Dictionary<string, string>()
        {
            {"Flag",   "false"},
            {"Int",    "0"},
            {"String", " "},
            {"Log",    "0"},
        };

        private const string OVERRIDE_STATUS_OFF = "No local overrides were found on load.";
        private const string OVERRIDE_STATUS_ON = "Values highlighted in red were overridden locally.";

        private List<FVariable> flags;
        private List<FVariable> allFlags;

        private static FVariable lastCustomFlag;
        private readonly Dictionary<string, int> flagLookup = new Dictionary<string, int>();

        private string currentSearch = "";
        private bool enterPressed = false;

        public FlagEditor()
        {
            InitializeComponent();

            overrideStatus.Text = OVERRIDE_STATUS_OFF;
            overrideStatus.Visible = false;

            overrideStatus.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                overrideTable?.Dispose();
                Program.SaveState();
            }
            
            base.Dispose(disposing);
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
            FVariable selectedFlag = lastCustomFlag;

            if (selectedFlag == null && flagDataGridView.SelectedRows.Count == 1)
            {
                DataGridViewRow selectedRow = flagDataGridView.SelectedRows[0];
                selectedFlag = flags[selectedRow.Index];
            }
            
            return selectedFlag?.Key == getFlagKeyByRow(row);
        }

        private void addFlagOverride(FVariable flag, bool init = false)
        {
            string key = flag.Key;

            if (!overrideRowLookup.ContainsKey(key))
            {
                if (!init && flag.Type.EndsWith("Flag", Program.StringFormat))
                {
                    if (bool.TryParse(flag.Value, out bool value))
                    {
                        string str = (!value)
                            .ToString(Program.Format)
                            .ToLower(Program.Format);

                        flag.SetValue(str);
                    }
                }

                DataRow row = overrideTable.Rows.Add
                (
                    flag.Name,
                    flag.Type,
                    flag.Value.ToLower(Program.Format)
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

                if (query.Any())
                {
                    // Select it.
                    var overrideRow = query.First();
                    overrideDataGridView.CurrentCell = overrideRow.Cells[0];
                }

                // Clear last custom flag.
                lastCustomFlag = null;

                // Switch to the overrides tab.
                tabs.SelectedTab = overridesTab;

                // Record this flag in the registry.
                flagRegistry[key] = flag;
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

        private async void InitializeEditor()
        {
            var flagNames = new HashSet<string>();

            var studioPath = StudioBootstrapper.GetStudioPath();
            FlagScanner.PerformScan(studioPath, flagNames);

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string settingsDir = Path.Combine(localAppData, "Roblox", "ClientSettings");

            string settingsPath = Path.Combine(settingsDir, "StudioAppSettings.json");
            string lastExecVersion = versionRegistry.LastExecutedVersion;
            string versionGuid = versionRegistry.VersionGuid;

            if (lastExecVersion != versionGuid)
            {
                // Reset the settings file.
                Directory.CreateDirectory(settingsDir);
                File.WriteAllText(settingsPath, "");

                // Create some system events for studio so we can hide the splash screen.
                using (var start = new SystemEvent("FFlagExtract"))
                using (var show = new SystemEvent("NoSplashScreen"))
                {
                    // Run Roblox Studio briefly so we can update the settings file.
                    ProcessStartInfo studioStartInfo = new ProcessStartInfo()
                    {
                        FileName = StudioBootstrapper.GetStudioPath(),
                        Arguments = $"-startEvent {start.Name} -showEvent {show.Name}"
                    };

                    Process studio = Process.Start(studioStartInfo);

                    var onStart = start.WaitForEvent();
                    await onStart.ConfigureAwait(true);

                    FileInfo info = new FileInfo(settingsPath);

                    // Wait for the settings path to be written.
                    while (info.Length == 0)
                    {
                        var delay = Task.Delay(30);
                        await delay.ConfigureAwait(true);

                        info.Refresh();
                    }

                    // Nuke studio and flag the version we updated with.
                    versionRegistry.LastExecutedVersion = versionGuid;
                    studio.Kill();
                }
            }

            // Initialize flag browser
            string[] flagNameStrings = Array.Empty<string>();

            string settings = File.ReadAllText(settingsPath);
            Dictionary<string, string> json;
            
            using (var reader = new StringReader(settings))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var data = JObject.Load(jsonReader);
                json = data.ToObject<Dictionary<string, string>>();
            }

            /*
            Dictionary<string, string> webJson;

            using (WebClient http = new WebClient())
            {
                http.Headers.Set("UserAgent", "RobloxStudioModManager");
                var rawData = await http.DownloadStringTaskAsync($"https://clientsettingscdn.roblox.com/v2/settings/application/PCStudioApp");

                using (var reader = new StringReader(rawData))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var data = JObject.Load(jsonReader);
                    var appSettings = data["applicationSettings"];
                    webJson = appSettings.ToObject<Dictionary<string, string>>();
                }
            }*/

            foreach (var key in flagNames)
            {
                if (!json.ContainsKey(key))
                {
                    string flagClass = "";

                    if (key.StartsWith("SF"))
                        flagClass = "SF";
                    else if (key.StartsWith("DF"))
                        flagClass = "DF";
                    else if (key.StartsWith("F"))
                        flagClass = "F";

                    if (flagClass != "")
                    {
                        var prefix = key.Substring(flagClass.Length);

                        foreach (var pair in flagTypes)
                        {
                            if (prefix.StartsWith(pair.Key))
                            {
                                json.Add(key, pair.Value);
                                break;
                            }
                        }
                    }
                    else
                    {
                        json.Add(key, "??");
                    }
                }
            }

            int numFlags = json.Count;
            var flagSetup = new List<FVariable>(numFlags);
            var autoComplete = new AutoCompleteStringCollection();

            foreach (string customFlag in flagNames)
            {
                if (!json.ContainsKey(customFlag))
                {
                    if (!flagRegistry.TryGetValue(customFlag, out var flag))
                        continue;

                    if (!flag.Custom)
                        continue;

                    flagSetup.Add(flag);
                }
            }

            foreach (var pair in json)
            {
                string key = pair.Key,
                       value = pair.Value;

                if (!flagRegistry.TryGetValue(key, out var flag))
                    flag = new FVariable(key, value);

                if (flag.Type == "")
                    continue;

                autoComplete.Add(flag.Key);
                flagSetup.Add(flag);
            }

            flagSearchFilter.AutoCompleteCustomSource = autoComplete;

            allFlags = flagSetup
                .OrderBy(flag => flag.Key)
                .ToList();

            refreshFlags();

            // Initialize override table.
            overrideTable = new DataTable();

            foreach (DataGridViewColumn column in overrideDataGridView.Columns)
                overrideTable.Columns.Add(column.DataPropertyName);

            var overrideView = new DataView(overrideTable) { Sort = "Name" };

            foreach (var flagName in flagRegistry.Keys)
            {
                var flag = flagRegistry[flagName];
                addFlagOverride(flag, true);
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

            var init = Task.Run(() =>
            {
                var initializer = new Action(InitializeEditor);
                Invoke(initializer);
            });

            await init.ConfigureAwait(true);

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

                FVariable flag = flagRegistry[flagKey];
                flag.Clear();

                if (overrideRowLookup.ContainsKey(flagKey))
                {
                    DataRow rowToDelete = overrideRowLookup[flagKey];
                    overrideRowLookup.Remove(flagKey);
                    rowToDelete.Delete();
                }

                selectedRow.Visible = false;
                selectedRow.Dispose();

                flagRegistry.Remove(flagKey);
            }

            if (overrideDataGridView.Rows.Count == 0)
            {
                overrideStatus.Text = OVERRIDE_STATUS_OFF;
                overrideStatus.ForeColor = Color.Black;
            }
        }

        private void removeAll_Click(object sender, EventArgs e)
        {
            bool doRemove = confirm("Confirmation", "Are you sure you would like to remove all flag overrides?\nThis will also delete any custom flags!");

            if (doRemove)
            {
                var flagNames = flagRegistry.Keys.ToList();

                foreach (string flagName in flagNames)
                {
                    var flag = flagRegistry[flagName];
                    flag.Clear();

                    flagRegistry.Remove(flagName);
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
            var format = Program.StringFormat;
            bool badInput = false;

            if (flagType.EndsWith("Flag", format))
            {
                string test = value
                    .ToUpperInvariant()
                    .Trim();

                badInput = (test != "FALSE" && test != "TRUE");
            }
            else if (flagType.EndsWith("Int", format))
            {
                badInput = !int.TryParse(value, out int _);
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

                if (flagType.EndsWith("Flag", Program.StringFormat) && cellType != typeof(DataGridViewComboBoxCell))
                {
                    // Switch the cell to a combo box.
                    // The user needs to select either true or false.
                    var newValueCell = new DataGridViewComboBoxCell();
                    newValueCell.Items.Add("true");
                    newValueCell.Items.Add("false");

                    newValueCell.Value = valueCell.Value
                        .ToString()
                        .ToLower(Program.Format);

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

                if (flagType.EndsWith("Flag", Program.StringFormat) && cellType != typeof(DataGridViewTextBoxCell))
                {
                    string value = valueCell.Value.ToString();
                    var newValueCell = new DataGridViewTextBoxCell() { Value = value };

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

                if (flagRegistry.ContainsKey(flag.Key))
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

        public static void ApplyFlags()
        {
            var json = new JObject();

            foreach (string flagName in flagRegistry.Keys)
            {
                var flag = flagRegistry[flagName];
                string value = flag.Value;

                var token = JToken.FromObject(value);
                json.Add(flagName, token);
            };

            string file = json.ToString();
            string studioDir = StudioBootstrapper.GetStudioDirectory();

            string clientSettings = Path.Combine(studioDir, "ClientSettings");
            Directory.CreateDirectory(clientSettings);

            string filePath = Path.Combine(clientSettings, "ClientAppSettings.json");
            File.WriteAllText(filePath, file);
        }

        private void addCustom_Click(object sender, EventArgs e)
        {
            using (FlagCreator flagCreator = new FlagCreator())
            {
                Enabled = false;
                UseWaitCursor = true;

                flagCreator.BringToFront();
                flagCreator.ShowDialog();
               
                Enabled = true;
                UseWaitCursor = false;

                if (flagCreator.DialogResult == DialogResult.OK)
                {
                    var customFlag = flagCreator.Result;
                    string flagType = customFlag.Type;

                    string flagValue = flagTypes
                        .Select(pair => pair.Key)
                        .Where(key => flagType.EndsWith(key, Program.StringFormat))
                        .Select(key => flagTypes[key])
                        .FirstOrDefault();

                    string flagName = flagType + customFlag.Name;
                    var newFlag = new FVariable(flagName, flagValue, true);

                    flagRegistry[flagName] = newFlag;
                    lastCustomFlag = newFlag;

                    addFlagOverride(newFlag);
                }
            }
        }
    }
}