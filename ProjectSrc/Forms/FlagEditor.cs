using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RobloxStudioModManager
{
    [SupportedOSPlatform("windows")]
    public partial class FlagEditor : Form
    {
        internal class ClientSettings
        {
            public Dictionary<string, string> ApplicationSettings = null;
        }

        internal class FlagDefaults
        {
            public int Format = 0;
            public Dictionary<string, string> Defaults = null;
        }

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

        private const string ENGINE_DEFAULTS_FILE = "FFlagDefaults.json";
        private const int ENGINE_DEFAULTS_FORMAT = 1;

        private const string CLIENT_SETTINGS_URL = "https://clientsettingscdn.roblox.com/v2/settings/application/PCDesktopClient";

        private List<FVariable> flags = new List<FVariable>();
        private List<FVariable> allFlags = new List<FVariable>();

        private static FVariable lastCustomFlag = null;
        private readonly Dictionary<string, int> flagLookup = new Dictionary<string, int>();
        private string currentSearch = "";

        public FlagEditor()
        {
            InitializeComponent();
            UseWaitCursor = true;
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
            return result == DialogResult.Yes;
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
            if (allFlags == null)
                return;

            string search = flagSearchFilter.Text.ToLowerInvariant();

            flags = allFlags
                .Where(flag => flag.Name.ToLowerInvariant().Contains(search))
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
            var currentCount = flagDataGridView.RowCount;

            if (currentCount == 0)
            {
                flagDataGridView.Rows.Add();
                currentCount = 1;
            }

            var diff = flags.Count - currentCount;
            flagDataGridView.SuspendLayout();

            if (diff > 0)
                flagDataGridView.Rows.AddCopies(0, diff);
            else if (diff < 0)
                flagDataGridView.RowCount += diff;

            flagDataGridView.ResumeLayout();
        }

        private static string getTypeDefault(string key)
        {
            string type = new FVariable(key, "").Type;

            foreach (var pair in flagTypes)
                if (type.EndsWith(pair.Key, Program.StringFormat))
                    return pair.Value;

            return "??";
        }

        // Maps each known flag to its engine default (null if it couldn't be determined).
        // Results are cached next to Studio and rescanned whenever the Studio version changes.
        private static Dictionary<string, string> loadEngineDefaults(string studioPath, string studioDir)
        {
            string versionGuid = versionRegistry.VersionGuid;
            string cachePath = Path.Combine(studioDir, ENGINE_DEFAULTS_FILE);

            if (versionRegistry.LastFlagScanVersion == versionGuid && File.Exists(cachePath))
            {
                try
                {
                    string json = File.ReadAllText(cachePath);
                    var data = JsonConvert.DeserializeObject<FlagDefaults>(json);

                    if (data.Format == ENGINE_DEFAULTS_FORMAT)
                    {
                        return data.Defaults;
                    }
                }
                catch (JsonException)
                {
                    // Fall through and rescan.
                }
            }

            var defaults = StudioFFlagDumper.DumpCppFlagDefaults(studioPath);
            var extraContentDir = Path.Combine(studioDir, "ExtraContent");

            if (Directory.Exists(extraContentDir))
                // DefineFast* only registers a flag if the engine hasn't already, so C++ defaults take priority.
                foreach (var pair in StudioFFlagDumper.DumpLuaFlagDefaults(extraContentDir))
                    defaults.TryAdd(pair.Key, pair.Value);

            var newDefaults = new FlagDefaults()
            {
                Format = ENGINE_DEFAULTS_FORMAT,
                Defaults = defaults
            };

            File.WriteAllText(cachePath, JsonConvert.SerializeObject(newDefaults));
            versionRegistry.LastFlagScanVersion = versionGuid;

            return defaults;
        }

        // Values served by the web JSON API, which override the engine defaults at runtime.
        private static async Task<Dictionary<string, string>> fetchWebOverrides()
        {
            var overrides = new Dictionary<string, string>();

            try
            {
                using (var http = new HttpClient())
                {
                    var settings = await http.GetStringAsync(CLIENT_SETTINGS_URL).ConfigureAwait(false);
                    var data = JsonConvert.DeserializeObject<ClientSettings>(settings);

                    if (data?.ApplicationSettings == null)
                        return overrides;

                    foreach (var pair in data.ApplicationSettings)
                    {
                        if (pair.Key.EndsWith("_PlaceFilter", Program.StringFormat))
                            continue;

                        overrides[pair.Key] = pair.Value;
                    }
                }
            }
            catch (Exception e) when (e is HttpRequestException || e is TaskCanceledException || e is JsonException)
            {
                // Offline or unexpected response, fall back to the engine defaults alone.
            }

            return overrides;
        }

        private async Task InitializeEditor()
        {
            var studioPath = StudioBootstrapper.GetStudioPath();
            var studioDir = StudioBootstrapper.GetStudioDirectory();

            var engineDefaults = await Task.Run(() => loadEngineDefaults(studioPath, studioDir));
            Program.SaveState();

            var webOverrides = await fetchWebOverrides();
            var flagSetup = new List<FVariable>(engineDefaults.Count);

            // Initialize flag browser
            foreach (var pair in engineDefaults)
            {
                string name = pair.Key;

                if (!webOverrides.TryGetValue(name, out string value))
                    value = pair.Value ?? getTypeDefault(name);

                var defaultFlag = new FVariable(name, value);

                if (defaultFlag.Type == "")
                    continue;

                if (flagRegistry.TryGetValue(defaultFlag.Key, out var flag))
                    flag.Reset = defaultFlag.Reset;
                else
                    flag = defaultFlag;

                flagSetup.Add(flag);
            }

            allFlags = flagSetup
                .OrderBy(flag => flag.Key)
                .ToList();

            var propInfo = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            propInfo.SetValue(flagDataGridView, true, null);
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
            TopMost = true;
            Enabled = false;
            UseWaitCursor = true;

            BringToFront();
            Refresh();

            await InitializeEditor();

            Enabled = true;
            UseWaitCursor = false;
        }

        private void flagDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            if (flags.Count == 0)
            {
                e.Value = "";
                return;
            }

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

        private void flagSearchFilter_TextChanged(object sender, EventArgs e)
        {
            if (flagSearchFilter.Text != currentSearch)
            {
                currentSearch = flagSearchFilter.Text;
                flagDataGridView.RowCount = 0;
                refreshFlags();
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
            var row = flagDataGridView.Rows[index];

            if (flags.Count == 0)
            {
                applyRowColor(row, Color.White);
                return;
            }

            FVariable flag = flags[index];

            if (flag.Dirty)
            {
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
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");

            string clientSettings = Path.Combine(localAppData, "Roblox", "ClientSettings");
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

        private void flagDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != 2)
                return;

            if (e.Button != MouseButtons.Right)
                return;

            var rowIndex = e.RowIndex;
            FVariable flag = flags[rowIndex];
            Clipboard.SetText(flag.Value);

            notification.BalloonTipTitle = flag.Name;
            notification.BalloonTipText = "Value copied to clipboard!";
            notification.ShowBalloonTip(2000);
        }
    }
}