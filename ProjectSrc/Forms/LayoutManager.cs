using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class LayoutManager : Form
    {
        private static readonly RegistryKey layoutRegistry = Program.GetSubKey("InterfaceLayouts");
        private static readonly RegistryKey presetRegistry = layoutRegistry.GetSubKey("LayoutPresets");

        private static readonly RegistryKey robloxRegistry = Registry.CurrentUser.GetSubKey("SOFTWARE", "Roblox");
        private static readonly RegistryKey robloxStudioRegistry = robloxRegistry.GetSubKey("RobloxStudio");

        private bool editDone = false;
        private int selectedIndex;
        private string[] savedPresets = Array.Empty<string>();

        private TextBox editBox;

        public LayoutManager()
        {
            InitializeComponent();
        }

        private void LayoutManager_Load(object sender, EventArgs e)
        {
            reloadPresetList();

            editBox = new System.Windows.Forms.TextBox();
            editBox.Location = new System.Drawing.Point(0, 0);
            editBox.Size = new System.Drawing.Size(0, 0);
            editBox.BorderStyle = BorderStyle.None;
            editBox.Hide();
            presetList.Controls.AddRange(new System.Windows.Forms.Control[] { this.editBox });
        }

        private void reloadPresetList()
        {
            presetList.Items.Clear();

            savedPresets = presetRegistry.GetValueNames();
            Array.Sort(savedPresets, StringComparer.InvariantCulture);

            foreach (var preset in savedPresets)
            {
                presetList.Items.Add(preset);
            }
        }

        private void createEditBox(object sender)
        {
            presetList = (ListBox)sender;
            selectedIndex = presetList.SelectedIndex;
            Rectangle r = presetList.GetItemRectangle(selectedIndex);
            string itemText = (string)presetList.Items[selectedIndex];
            editBox.Location = new System.Drawing.Point(r.X, r.Y);
            editBox.Size = new System.Drawing.Size(r.Width, r.Height);
            editBox.Show();
            presetList.Controls.AddRange(new System.Windows.Forms.Control[] { this.editBox });
            editBox.Text = itemText;
            editBox.Focus();
            editBox.SelectAll();
            editBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editOver);
            editBox.LostFocus += new System.EventHandler(this.focusOver);
        }

        private void focusOver(object sender, EventArgs e)
        {
            if (!editDone)
            {
                editDone = true;

                if(presetRegistry.GetValue(editBox.Text) != null)
                {
                    editBox.Hide();
                    return;
                }

                string regKey = savedPresets[selectedIndex];
                byte[] preset = presetRegistry.GetValue(regKey) as byte[];

                presetRegistry.DeleteValue(regKey);
                presetRegistry.SetValue(editBox.Text, preset, RegistryValueKind.Binary);

                loadSelectedLayout.Enabled = false;
                deleteSelectedLayout.Enabled = false;

                reloadPresetList();
                editBox.Hide();
            }
        }

        private void editOver(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                focusOver(sender, e);
            }
        }

        private void presetList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex = presetList.SelectedIndex;

            loadSelectedLayout.Enabled = selectedIndex != -1;
            deleteSelectedLayout.Enabled = selectedIndex != -1;
        }

        private void presetList_DoubleClick(object sender, EventArgs e)
        {
            editDone = false;
            createEditBox(sender);
        }

        private void saveCurrentLayout_Click(object sender, EventArgs e)
        {
            byte[] currentLayout = robloxStudioRegistry.GetValue("window_state_ribbon") as byte[];

            int index = presetRegistry.GetValueNames().Length + 1;
            while(presetRegistry.GetValue("New Preset #" + index) != null)
            {
                index++;
            }

            presetRegistry.SetValue("New Preset #" + index, currentLayout, RegistryValueKind.Binary);

            reloadPresetList();
        }

        private void deleteSelectedLayout_Click(object sender, EventArgs e)
        {
            string regKey = savedPresets[selectedIndex];

            presetRegistry.DeleteValue(regKey);

            loadSelectedLayout.Enabled = false;
            deleteSelectedLayout.Enabled = false;

            reloadPresetList();
        }

        private void loadSelectedLayout_Click(object sender, EventArgs e)
        {
            string regKey = savedPresets[selectedIndex];
            byte[] savedLayout = presetRegistry.GetValue(regKey) as byte[];

            robloxStudioRegistry.SetValue("window_state_ribbon", savedLayout, RegistryValueKind.Binary);

            presetList.SelectedIndex = -1;
            loadSelectedLayout.Enabled = false;
            deleteSelectedLayout.Enabled = false;
        }

        /*
        private void saveLayout_Click(object sender, EventArgs e)
        {
            byte[] currentLayout = robloxStudioRegistry.GetValue("window_state_ribbon") as byte[];

            presetRegistry.SetValue("main", currentLayout, RegistryValueKind.Binary);

            updateLoadEnabled();
        }

        private void loadLayout_Click(object sender, EventArgs e)
        {
            byte[] savedLayout = presetRegistry.GetValue("main") as byte[];

            robloxStudioRegistry.SetValue("window_state_ribbon", savedLayout, RegistryValueKind.Binary);
        }
        */
    }
}
