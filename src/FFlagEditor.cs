using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxModManager
{
    public partial class FFlagEditor : Form
    {
        private Launcher launcher;
        private string database;
        private static List<string> fflags;
        private static bool fflagsLoaded = false;
        private static RegistryKey fflagRegistry = Program.GetSubKey(Program.ModManagerRegistry, "FFlags");

        public FFlagEditor(Launcher launcher, string database)
        {
            this.launcher = launcher;
            this.database = database;

            launcher.Enabled = false;
            InitializeComponent();
        }

        private void fatalError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        private async Task setStatus(string msg)
        {
            statusLbl.Text = msg;
            await Task.Delay(1);
        }

        private void reassembleListings()
        {
            string filter = searchFilter.Text.ToLower();

            foreach (string fflag in fflags)
            {
                bool shouldAdd = fflag.ToLower().Contains(filter);
                if (shouldAdd && !availableFFlags.Items.Contains(fflag))
                    availableFFlags.Items.Add(fflag);
                else if (!shouldAdd && availableFFlags.Items.Contains(fflag))
                    availableFFlags.Items.Remove(fflag);
            }

            foreach (string key in fflagRegistry.GetValueNames())
            {
                bool shouldHave = key.ToLower().Contains(filter.ToLower());

                if (shouldHave && !fflagConfig.Items.Contains(key))
                {
                    string strValue = fflagRegistry.GetValue(key, "") as string;
                    bool value = (strValue == "True" ? true : false);
                    fflagConfig.Items.Add(key, value);
                    availableFFlags.Items.Remove(key);
                }
                else if (!shouldHave && fflagConfig.Items.Contains(key))
                {
                    fflagConfig.Items.Remove(key);
                }
            }
        }

        private async void FFlagEditor_Load(object sender, EventArgs e)
        {
            if (!fflagsLoaded)
            {
                try
                {
                    Enabled = false;

                    await setStatus("Fetching current Roblox Studio build, this might take a moment...");

                    WebClient http = new WebClient();
                    http.Headers.Set(HttpRequestHeader.UserAgent, "Roblox");

                    string setup = "https://setup." + database + ".com/";
                    string version = await http.DownloadStringTaskAsync(setup + "versionQTStudio");
                    byte[] build = await http.DownloadDataTaskAsync(setup + version + "-RobloxStudio.zip");

                    await setStatus("Extracting Roblox Studio...");

                    Stream stream = new MemoryStream(build);
                    ZipArchive zipFile = new ZipArchive(stream);
                    ZipArchiveEntry studioArchive = zipFile.GetEntry("RobloxStudioBeta.exe");

                    fflags = new List<string>();
                    using (Stream studioStream = studioArchive.Open())
                    {
                        using (StreamReader reader = new StreamReader(studioStream))
                        {
                            string studio = reader.ReadToEnd();
                            await setStatus("Fetching Available FFlags...");
                            MatchCollection matches = Regex.Matches(studio, "PlaceFilter_[a-zA-Z0-9_]+");
                            foreach (Match match in matches)
                            {
                                string placeFilterFlag = match.Groups[0].ToString();
                                string fflag = placeFilterFlag.Replace("PlaceFilter_", "");
                                fflags.Add(fflag);
                            }
                        }
                    }

                    await setStatus("Loading Layout...");
                    reassembleListings();

                    await setStatus("Ready!");
                    Enabled = true;
                    fflagsLoaded = true;
                }
                catch (Exception ex)
                {
                    fatalError("Exception thrown while loading FFlag Editor: " + ex.Message);
                }
            }
            else
            {
                reassembleListings();
            }
        }

        private void FFlagEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            launcher.Enabled = true;
            launcher.Show();
            launcher.BringToFront();
        }

        private async void availableFFlags_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = availableFFlags.SelectedIndex;
            add.Enabled = (selected >= 0);
            if (add.Enabled)
            {
                string fflag = availableFFlags.Items[selected] as string;
                fflagConfig.SelectedIndex = -1;
                await setStatus("Selected: " + fflag);
            }
        }

        private void add_Click(object sender, EventArgs e)
        {
            int selected = availableFFlags.SelectedIndex;
            string fflag = availableFFlags.Items[selected] as string;
            DialogResult result = MessageBox.Show("Should " + fflag + " be enabled?", "Initial State", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            bool enabled = (result == DialogResult.Yes);
            fflagConfig.Items.Add(fflag, enabled);
            fflagRegistry.SetValue(fflag, enabled);
            availableFFlags.Items.Remove(fflag);
        }

        private async void fflagConfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = fflagConfig.SelectedIndex;
            remove.Enabled = (selected >= 0);
            if (remove.Enabled)
            {
                string fflag = fflagConfig.Items[selected] as string;
                availableFFlags.SelectedIndex = -1;
                await setStatus("Selected: " + fflag);
            }
        }

        private void fflagConfig_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string fflag = fflagConfig.Items[e.Index] as string;
            bool isChecked = (e.NewValue == CheckState.Checked);
            fflagRegistry.SetValue(fflag, isChecked);
        }

        private void remove_Click(object sender, EventArgs e)
        {
            int selected = fflagConfig.SelectedIndex;
            string fflag = fflagConfig.Items[selected] as string;
            fflagConfig.Items.Remove(fflag);

            if (fflags.Contains(fflag))
                availableFFlags.Items.Add(fflag);

            fflagRegistry.DeleteValue(fflag);
        }

        private void searchFilter_TextChanged(object sender, EventArgs e)
        {
            statusLbl.Text = "";
            reassembleListings();
        }
    }
}
