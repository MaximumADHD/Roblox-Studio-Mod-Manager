using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class FVariableEditor : Form
    {
        private Launcher launcher;
        private string database;
        private bool submittedAdvFVarValue = false;
        private static List<string> fvars;
        private static bool fvarsLoaded = false;
        private static RegistryKey fvarRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FVariables");

        private delegate void reassembleListingsDelegate(object sender = null, EventArgs e = null);
        private delegate void voidDelegate();

        public FVariableEditor(Launcher launcher, string database)
        {
            this.launcher = launcher;
            this.database = database;
            launcher.Enabled = false;
            InitializeComponent();
        }

        private void error(string msg, bool fatal = false)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (fatal) Close();
        }

        private async Task setStatus(string msg)
        {
            statusLbl.Text = msg;
            await Task.Delay(1);
        }

        private RegistryKey getFVarEntry(string fvar)
        {
            return Program.GetSubKey(fvarRegistry, fvar);
        }

        private void reassembleListings(object sender = null, EventArgs e = null)
        {
            string filter = searchFilter.Text.ToLower();

            foreach (string fvar in fvars)
            {
                bool shouldAdd = fvar.ToLower().Contains(filter);
                if (shouldAdd && !availableFVariables.Items.Contains(fvar))
                    availableFVariables.Items.Add(fvar);
                else if (!shouldAdd && availableFVariables.Items.Contains(fvar))
                    availableFVariables.Items.Remove(fvar);
            }

            fvarConfig.SelectedIndex = -1;
            advFVarSelect.AutoCompleteCustomSource.Clear();
            advFVarSelect.Items.Clear();
            advFVarSelect.Text = "";
            advFVarType.SelectedIndex = -1;
            advFVarValue.Text = "";
            advFVarValue.Enabled = false;

            statusLbl.Text = "Select an FVariable to continue.";

            foreach (string fvarName in fvarRegistry.GetSubKeyNames())
            {
                RegistryKey fvarEntry = Program.GetSubKey(fvarRegistry, fvarName);

                if (availableFVariables.Items.Contains(fvarName))
                    availableFVariables.Items.Remove(fvarName);

                if (fvarConfig.Items.Contains(fvarName))
                    fvarConfig.Items.Remove(fvarName);

                string type = fvarEntry.GetValue("Type") as string;
                if (type == "") type = "Flag";

                string strValue = fvarEntry.GetValue("Value","?") as string;
                CheckState state = CheckState.Indeterminate;

                if (type == "Flag")
                {
                    switch (strValue.ToLower())
                    {
                        case "true":
                            state = CheckState.Checked;
                            break;
                        case "false":
                            state = CheckState.Unchecked;
                            break;
                    }
                }

                fvarConfig.Items.Add(fvarName, state);
            }

            foreach (string fvar in fvarConfig.Items)
            {
                advFVarSelect.Items.Add(fvar);
                advFVarSelect.AutoCompleteCustomSource.Add(fvar);
            }

            foreach (string fvar in availableFVariables.Items)
                advFVarSelect.AutoCompleteCustomSource.Add(fvar);
                

            advFVarSelect.SelectedIndex = -1;
        }

        private void submitFlagToQueue(string fvar)
        {
            DialogResult result = MessageBox.Show("Should " + fvar + " be enabled?", "Initial State", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            bool enabled = (result == DialogResult.Yes);

            if (!fvarConfig.Items.Contains(fvar))
                fvarConfig.Items.Add(fvar, enabled);

            advFVarSelect.Items.Add(fvar);

            RegistryKey fvarEntry = getFVarEntry(fvar);
            fvarEntry.SetValue("Type", "Flag");
            fvarEntry.SetValue("Value", enabled);

            if (availableFVariables.Items.Contains(fvar))
                availableFVariables.Items.Remove(fvar);

            reassembleListings();
        }

        private async void FVariableEditor_Load(object sender, EventArgs e)
        {
            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string exePath = Path.Combine(localAppData, "Roblox Studio", "RobloxStudioBeta.exe");
            string installedDatabase = Program.ModManagerRegistry.GetValue("BuildDatabase") as string;

            bool installFirst = true;

            if (!File.Exists(exePath))
            {
                MessageBox.Show("Roblox Studio will be installed prior to editing so that FVariable scans can be done.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (database != installedDatabase)
            {
                DialogResult result = MessageBox.Show("The branch you have selected hasn't been installed, and thus FVariable scans will not be representative of this branch.\nWould you like to install it now?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    installFirst = false;
            }
            else
            {
                installFirst = false;
            }

            if (installFirst)
            {
                fvarsLoaded = false;
                using (RobloxInstaller installer = new RobloxInstaller(!File.Exists(exePath)))
                {
                    Enabled = false;
                    SendToBack();
                    await installer.RunInstaller(database,true);
                    Enabled = true;
                    BringToFront();
                }
            }

            if (!fvarsLoaded)
            {
                if (!File.Exists(exePath))
                {
                    error("Could not find Roblox Studio's exe, did you close the installer before it finished?", true);
                    return;
                }

                try
                {
                    Enabled = false;
                    fvars = new List<string>();

                    foreach (string fvar in fvarRegistry.GetSubKeyNames())
                    {
                        if (!fvars.Contains(fvar))
                        {
                            RegistryKey fvarEntry = getFVarEntry(fvar);
                            bool custom = bool.Parse(fvarEntry.GetValue("Custom", "false") as string);
                            if (custom) fvars.Add(fvar);
                        }
                    }

                    RegistryKey fvarScan = Program.ModManagerRegistry.CreateSubKey("SavedFVariableScan");
                    foreach (string fvar in fvarScan.GetValueNames())
                    {
                        if (!fvars.Contains(fvar))
                            fvars.Add(fvar);
                    }

                    string fvarProtocol = Program.ModManagerRegistry.GetValue("FVariable Protocol Version", "v0") as string;
                    int version = int.Parse(fvarProtocol.Substring(1));
                    if (version < 1)
                    {
                        // Convert any legacy registry keys over to the new system.
                        RegistryKey fflagRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FFlags");
                        foreach (string key in fflagRegistry.GetValueNames())
                        {
                            string value = fflagRegistry.GetValue(key) as string;
                            RegistryKey fvarEntry = getFVarEntry(key);
                            fvarEntry.SetValue("Type", "Flag");
                            fvarEntry.SetValue("Value", value);
                        }
                        Program.ModManagerRegistry.DeleteSubKey("FFlags");

                        version = 1;
                        Program.ModManagerRegistry.SetValue("FVariable Protocol Version", "v1");
                    }

                    await setStatus("Loading Layout...");
                    reassembleListings();

                    await setStatus("Ready!");
                    Enabled = true;
                    fvarsLoaded = true;
                }
                catch (Exception ex)
                {
                    error("Exception thrown while loading FVariable Editor: " + ex.Message + '\n' + ex.StackTrace,true);
                }
            }
            else
            {
                reassembleListings();
            }
        }

        private void FVariableEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            launcher.Enabled = true;
            launcher.Show();
            launcher.BringToFront();
        }

        private async void availableFVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = availableFVariables.SelectedIndex;
            add.Enabled = (selected >= 0);
            if (add.Enabled)
            {
                string fvar = availableFVariables.Items[selected] as string;
                fvarConfig.SelectedIndex = -1;
                advFVarSelect.SelectedIndex = -1;
                await setStatus("Selected: " + fvar);
            }
        }

        private void add_Click(object sender, EventArgs e)
        {
            int selected = availableFVariables.SelectedIndex;
            string fvar = availableFVariables.Items[selected] as string;
            submitFlagToQueue(fvar);
        }

        private async void fvarConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = fvarConfig.SelectedIndex;
            remove.Enabled = (selected >= 0);
            if (remove.Enabled)
            {
                string fvar = fvarConfig.Items[selected] as string;
                availableFVariables.SelectedIndex = -1;
                RegistryKey fvarEntry = getFVarEntry(fvar);
                string type = fvarEntry.GetValue("Type") as string;
                string value = fvarEntry.GetValue("Value") as string;
                await setStatus("Selected: F" + type + fvar + " (Value = " + value + ")");
            }
        }

        private void fvarConfig_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string fvar = fvarConfig.Items[e.Index] as string;
            RegistryKey fvarEntry = getFVarEntry(fvar);
            string type = fvarEntry.GetValue("Type") as string;
            if (type != "Flag" && type != null)
            {
                if (e.NewValue != CheckState.Indeterminate)
                {
                    fvarConfigTabs.SelectedTab = advFVariableConfig;
                    advFVarSelect.Text = fvar;
                    e.NewValue = CheckState.Indeterminate;
                    ActiveControl = advFVarValue;
                    advFVarValue.SelectionStart = fvar.Length - 1;
                }
            }
            else
            {
                bool value = (e.NewValue == CheckState.Checked ? true : false);
                fvarEntry.SetValue("Value", value);
                fvarEntry.SetValue("Type", "Flag");
            }
        }

        private void remove_Click(object sender, EventArgs e)
        {
            int selected = fvarConfig.SelectedIndex;
            string fvar = fvarConfig.Items[selected] as string;
            fvarConfig.Items.Remove(fvar);

            if (fvars.Contains(fvar))
                availableFVariables.Items.Add(fvar);

            fvarRegistry.DeleteSubKey(fvar);
            reassembleListings();
        }

        private void validateEntryValue(bool submitted = false)
        {
            string fvar = advFVarSelect.Text;
            string type = advFVarType.Text;
            string currentValue = advFVarValue.Text;
            string newValue = currentValue;
            if (type == "Flag")
            {
                bool result = false;
                bool.TryParse(currentValue, out result);
                newValue = result.ToString();
            }
            else if (type != "String")
            {
                int result = 0;
                int.TryParse(currentValue, out result);
                newValue = result.ToString();
            }
            if (currentValue.ToLower() != newValue.ToLower())
            {
                if (submitted)
                {
                    error("Bad input.");
                    reassembleListings();
                }
                advFVarValue.Text = newValue;
            }   
        }

        private async void advFVarValue_Leave(object sender, EventArgs e)
        {
            string fvar = advFVarSelect.Text;
            RegistryKey fvarEntry = getFVarEntry(fvar);
            if (submittedAdvFVarValue)
            {
                submittedAdvFVarValue = false;
                fvarEntry.SetValue("Value", advFVarValue.Text);
                validateEntryValue(true);
            }
            else
            {
                string value = fvarEntry.GetValue("Value") as string;
                advFVarValue.Text = value;
            }
            await setStatus("(Set Value: F" + advFVarType.Text + fvar + " = " + advFVarValue.Text + ")");
        }

        private void advFVarValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                submittedAdvFVarValue = true;
                ActiveControl = null;
            }
        }

        private async void advFVarType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (advFVarType.SelectedIndex >= 0)
            {
                string fvar = advFVarSelect.Text;
                RegistryKey fvarEntry = getFVarEntry(fvar);
                string type = advFVarType.Items[advFVarType.SelectedIndex] as string;
                fvarEntry.SetValue("Type", type);
                await setStatus("(Key set as: F" + type + fvar + ")");
                validateEntryValue();
            }
        }

        private void advFVarSelect_Enter(object sender, EventArgs e)
        {
            availableFVariables.SelectedIndex = -1;
        }

        private void advFVarSelect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                string fvar = advFVarSelect.Text;
                searchFilter.Text = "";
                
                int index = advFVarSelect.Items.IndexOf(fvar);
                if (index >= 0)
                    advFVarSelect.SelectedIndex = index;
                else if (fvars.Contains(fvar))
                {
                    submitFlagToQueue(fvar);
                    advFVarSelect.SelectedText = fvar;
                }   
                else
                {
                    if (fvar.Length > 0)
                    {
                        DialogResult result = MessageBox.Show("You are submitting a FVariable that wasn't found in the available listing.\nAre you sure you want to add this?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            submitFlagToQueue(fvar);
                            RegistryKey fvarEntry = getFVarEntry(fvar);
                            fvarEntry.SetValue("Custom", true);
                            advFVarSelect.SelectedIndex = advFVarSelect.Items.IndexOf(fvar);
                        }
                        else
                        {
                            reassembleListings();
                        }
                    }
                    else
                    {
                        error("Invalid input.");
                    }
                }
            }
        }

        private async void advFVarSelect_TextChanged(object sender, EventArgs e)
        {
            bool enabled = fvarConfig.Items.Contains(advFVarSelect.Text);
            advFVarType.Enabled = enabled;
            advFVarValue.Enabled = enabled;
            if (enabled)
            {
                string fvar = advFVarSelect.Text;
                RegistryKey fvarEntry = getFVarEntry(fvar);
                advFVarType.SelectedIndex = advFVarType.Items.IndexOf(fvarEntry.GetValue("Type", "Flag"));
                if (advFVarType.SelectedIndex == -1) // Corrupted?
                    advFVarType.SelectedIndex = 0;

                advFVarValue.Text = fvarEntry.GetValue("Value") as string;
                await setStatus("Selected: F" + advFVarType.Text + fvar);
                validateEntryValue();
            }
            else
            {
                advFVarType.SelectedIndex = -1;
                advFVarValue.Text = "";
            }
        }

        private void scanBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("An FVariable scan will run an invisible instance of Roblox Studio that communicates with the program through the HttpService.\n\nThis may take a few moments to scan, but the results will persist between sessions.\n\nDo you want to continue?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Enabled = false;
                FVariableScanner fetcher = new FVariableScanner(this);
                fetcher.Show();
                fetcher.BringToFront();
            }
        }

        private void showSuccessMessage()
        {
            MessageBox.Show(this,"Scan completed successfully!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void receiveFVariableScan(List<string> newFVars)
        {
            fvars = newFVars;

            Program.ModManagerRegistry.DeleteSubKey("SavedFVariableScan");
            RegistryKey fvarScan = Program.ModManagerRegistry.CreateSubKey("SavedFVariableScan");
            foreach (string fvar in fvars)
                fvarScan.SetValue(fvar, '\0');

            Invoke(new voidDelegate(Show));
            Invoke(new voidDelegate(BringToFront));
            Invoke(new voidDelegate(showSuccessMessage));
            Invoke(new reassembleListingsDelegate(reassembleListings), null, null);
        }
    }
}
