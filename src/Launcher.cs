using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;

namespace RobloxModManager
{
    public partial class Launcher : Form
    {
        private WebClient http = new WebClient();
        private string[] args = null;

        public string getModPath()
        {
            string appData = Environment.GetEnvironmentVariable("AppData");
            string root = Path.Combine(appData, "RbxModManager", "ModFiles");
            if (!Directory.Exists(root))
            {
                // Build a folder structure so the usage of my mod manager is more clear.
                string[] folderPaths = new string[]
                {
                    "BuiltInPlugins",
                    "ClientSettings",
                    "content/fonts",
                    "content/music",
                    "content/particles",
                    "content/scripts",
                    "content/sky",
                    "content/sounds",
                    "content/textures",
                };
                foreach (string f in folderPaths)
                {
                    string path = Path.Combine(root,f.Replace("/", "\\"));
                    Directory.CreateDirectory(path);
                }
            }
            return root;
        }

        public string getOpenVR(string directory)
        {
            string openVR = Path.Combine(directory, "openvr_api.dll");
            if (!File.Exists(openVR))
            {
                openVR = openVR + "_disabled";
                if (!File.Exists(openVR))
                {
                    Console.WriteLine("ERROR: Could not find openvr_api");
                    throw new Exception();
                }
            }
            return openVR;
        }

        private void manageMods_Click(object sender, EventArgs e)
        {
            string modPath = getModPath();
            Process.Start(modPath);
        }

        private async void launchStudio_Click(object sender = null, EventArgs e = null)
        {
            this.Hide();
            string dataBase = (string)dataBaseSelect.Items[Properties.Settings.Default.Database];
            RobloxInstaller installer = new RobloxInstaller();
            string studioPath = await installer.RunInstaller(dataBase,forceRebuild.Checked);
            string studioRoot = Directory.GetParent(studioPath).ToString();
            string modPath = getModPath();
            string[] studioFiles = Directory.GetFiles(studioRoot);
            string[] modFiles = Directory.GetFiles(modPath,"*.*",SearchOption.AllDirectories);
            foreach (string modFile in modFiles)
            {
                try
                {
                    byte[] fileContents = File.ReadAllBytes(modFile);
                    FileInfo modFileControl = new FileInfo(modFile);
                    string relativeFile = modFile.Replace(modPath, studioRoot);
                    string relativeDir = Directory.GetParent(relativeFile).ToString();
                    if (!Directory.Exists(relativeDir))
                    {
                        Directory.CreateDirectory(relativeDir);
                    }
                    if (!File.Exists(relativeFile))
                    {
                        FileStream currentStream = File.Create(relativeFile);
                        currentStream.Close();
                    }
                    byte[] studioFile = File.ReadAllBytes(relativeFile);
                    if (!fileContents.SequenceEqual(studioFile))
                    {
                        modFileControl.CopyTo(relativeFile, true);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to overwrite " + modFile + "\nMight be open in another program\nThats their problem, not mine <3");
                }
            }
            ProcessStartInfo robloxStudioInfo = new ProcessStartInfo();
            robloxStudioInfo.FileName = studioPath;
            if (args != null)
            {
                string firstArg = args[0];
                if (firstArg != null)
                {
                    if (firstArg.StartsWith("roblox-studio"))
                    {
                        foreach (string commandPair in firstArg.Split('+'))
                        {
                            if (commandPair.Contains(':'))
                            {
                                string[] keyVal = commandPair.Split(':');
                                string key = keyVal[0];
                                string val = keyVal[1];
                                if (key == "script")
                                {
                                    robloxStudioInfo.Arguments = "-script " + WebUtility.UrlDecode(val);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        robloxStudioInfo.Arguments = string.Join(" ", args);
                    }
                }
            }
            else
            {
                this.Dispose();
            }
            Process robloxStudio = Process.Start(robloxStudioInfo);
            if (openStudioDirectory.Checked)
            {
                Process.Start(studioRoot);
            }
            installer.Dispose();
            robloxStudio.WaitForExit();
            Application.Exit();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            dataBaseSelect.SelectedIndex = Properties.Settings.Default.Database;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Database = dataBaseSelect.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void onHelpRequested(object sender, HelpEventArgs e)
        {
            Control controller = sender as Control;
            string msg = null;
            if (sender.Equals(pictureBox1))
            {
                msg = "This is just the Roblox Studio logo.\nNothing special here.";
            }
            else if (sender.Equals(launchStudio))
            {
                msg = "Click to Launch Roblox Studio!";
            }
            else if (sender.Equals(manageMods))
            {
                msg = "Opens your ModFolder directory, which contains all of the files to be overridden in Roblox Studio's client directory.";
            }
            else if (sender.Equals(dataBaseSelect))
            {
                msg = "Indicates which setup web-domain we should use to download Roblox Studio.\nThe gametest domains are prototype versions of ROBLOX Studio,\nthat are not available on the main site yet.";
            }
            else if (sender.Equals(forceRebuild))
            {
                msg = "Should we forcefully reinstall this version of the client, even if its already installed?\nThis can be used if you are experiencing a problem with launching Roblox Studio.";
            }
            else if (sender.Equals(openStudioDirectory))
            {
                msg = "Should we also open the directory of Roblox Studio after launching?\nThis may come in handy for users who want to run .bat on Studio's files.";
            }
            else
            {
                msg = "Error: Unknown Sender " + controller.Name;
            }
            if (msg != null)
            {
                MessageBox.Show(msg, "Information about the \"" + controller.AccessibleName + "\" " + sender.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Handled = true;
            }
        }

        public Launcher(params string[] mainArgs)
        {
            if (mainArgs.Length > 0)
            {
                args = mainArgs;
            }
            InitializeComponent();
        }
    }
}