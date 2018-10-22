using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Windows.Forms;

namespace RobloxStudioModManager
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

                    "content/avatar",
                    "content/fonts",
                    "content/models",
                    "content/scripts",
                    "content/sky",
                    "content/sounds",
                    "content/textures",
                    "content/translations"
                };

                foreach (string f in folderPaths)
                {
                    string path = Path.Combine(root, f.Replace("/", "\\"));
                    Directory.CreateDirectory(path);
                }
            }

            return root;
        }

        private void manageMods_Click(object sender, EventArgs e)
        {
            string modPath = getModPath();
            Process.Start(modPath);
        }

        private void editFVariables_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Editing flags can make Roblox Studio unstable, and could potentially corrupt your places and game data.\n\nYou should not edit them unless you're just experimenting locally, and you know what you're doing.\n\nAre you sure you would like to continue?", "WARNING: HERE BE DRAGONS", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
            if (result == DialogResult.Yes)
            {
                string branch = (string)branchSelect.SelectedItem;
                FlagEditor editor = new FlagEditor(this, branch);
                editor.Show();
            }
        }

        private async void launchStudio_Click(object sender = null, EventArgs e = null)
        {
            Hide();

            string branch = (string)branchSelect.SelectedItem;
            RobloxInstaller installer = new RobloxInstaller();

            string studioPath = await installer.RunInstaller(branch, forceRebuild.Checked);
            string studioRoot = Directory.GetParent(studioPath).ToString();
            string modPath = getModPath();

            string[] studioFiles = Directory.GetFiles(studioRoot);
            string[] modFiles = Directory.GetFiles(modPath, "*.*", SearchOption.AllDirectories);

            foreach (string modFile in modFiles)
            {
                try
                {
                    byte[] fileContents = File.ReadAllBytes(modFile);
                    FileInfo modFileControl = new FileInfo(modFile);

                    bool allow = true;

                    if (modFileControl.Name == "ClientAppSettings.json")
                    {
                        DialogResult result = MessageBox.Show("A custom ClientAppSettings configuration was detected in your mods folder. This will override the configuration provided by the FVariable Editor.\nAre you sure you want to use this one instead?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.No)
                            allow = false;
                    }

                    if (allow)
                    {
                        string relativeFile = modFile.Replace(modPath, studioRoot);
                        string relativeDir = Directory.GetParent(relativeFile).ToString();

                        if (!Directory.Exists(relativeDir))
                            Directory.CreateDirectory(relativeDir);

                        if (File.Exists(relativeFile))
                        {
                            byte[] relativeContents = File.ReadAllBytes(relativeFile);
                            if (!fileContents.SequenceEqual(relativeContents))
                                modFileControl.CopyTo(relativeFile, true);
                        }
                        else
                        {
                            File.WriteAllBytes(relativeFile, fileContents);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to overwrite " + modFile + "\nMight be open in another program\nThats their problem, not mine <3");
                }
            }

            // Hack in the metadata plugin.
            // This is used to provide an end-point to custom StarterScripts that are trying to fork what branch they are on.
            // It creates a BindableFunction inside of the ScriptContext called GetModManagerBranch, which returns the branch set in the launcher.

            try
            {
                Assembly self = Assembly.GetExecutingAssembly();
                string metaScript;

                using (Stream stream = self.GetManifestResourceStream("RobloxStudioModManager.Resources.ModManagerMetadata.lua"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    metaScript = reader.ReadToEnd();
                    metaScript = metaScript.Replace("%MOD_MANAGER_VERSION%", '"' + branch + '"'); // TODO: Make this something more generic?
                }

                string dir = Path.Combine(studioRoot, "BuiltInPlugins");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string metaScriptFile = Path.Combine(dir, "__rbxModManagerMetadata.lua");
                File.WriteAllText(metaScriptFile, metaScript);
            }
            catch
            {
                Console.WriteLine("Failed to write __rbxModManagerMetadata.lua");
            }

            ProcessStartInfo robloxStudioInfo = new ProcessStartInfo();
            robloxStudioInfo.FileName = studioPath;

            if (args != null)
            {
                string firstArg = args[0];
                var argMap = new Dictionary<string, string>();

                if (firstArg != null && firstArg.StartsWith("roblox-studio"))
                {
                    foreach (string commandPair in firstArg.Split('+'))
                    {
                        if (commandPair.Contains(':'))
                        {
                            string[] kvPair = commandPair.Split(':');

                            string key = kvPair[0];
                            string val = kvPair[1];

                            argMap.Add(key, val);
                            robloxStudioInfo.Arguments += " -" + key + ' ' + val;
                        }
                    }

                    if (argMap.ContainsKey("launchmode") && !argMap.ContainsKey("task"))
                    {
                        robloxStudioInfo.Arguments += "-task ";

                        string launchMode = argMap["launchmode"];
                        if (launchMode == "plugin")
                            robloxStudioInfo.Arguments += "InstallPlugin";
                        else if (launchMode == "edit")
                            robloxStudioInfo.Arguments += "EditPlace";

                    }
                }
                else
                {
                    string fullArg = string.Join(" ", args);
                    robloxStudioInfo.Arguments += fullArg;
                }
            }

            if (openStudioDirectory.Checked)
            {
                Process.Start(studioRoot);
            }
            else
            {
                string currentVersion = Program.GetRegistryString(Program.ModManagerRegistry, "BuildVersion");
                Program.ModManagerRegistry.SetValue("LastExecutedVersion", currentVersion);
                Process.Start(robloxStudioInfo);
            }
            
            Environment.Exit(0);
        }

        private void Launcher_Load(object sender, EventArgs e)
        {

            if (args != null)
                openStudioDirectory.Enabled = false;

            object registrySave = Program.ModManagerRegistry.GetValue("BuildBranch");

            if (registrySave != null)
            {
                string build = registrySave as string;
                branchSelect.SelectedIndex = branchSelect.Items.IndexOf(build);
            }
            else
            {
                branchSelect.SelectedIndex = 0;
            }
        }

        private void onHelpRequested(object sender, HelpEventArgs e)
        {
            Control controller = sender as Control;
            string msg = null;
            if (sender.Equals(launchStudio))
                msg = "Click to Launch Roblox Studio!";
            else if (sender.Equals(manageMods))
                msg = "Opens your ModFolder directory, which contains all of the files to be overridden in Roblox Studio's client directory.";
            else if (sender.Equals(branchSelect))
                msg = "Indicates which setup web-domain we should use to download Roblox Studio.\nThe gametest domains are QA testing versions of Roblox Studio that are not available on production yet.";
            else if (sender.Equals(forceRebuild))
                msg = "Should the mod manager forcefully reinstall this version of the client, even if its already installed?\nThis can be used if you are experiencing a problem with launching Roblox Studio.";
            else if (sender.Equals(openStudioDirectory))
                msg = "Should the mod manager just open the directory of Roblox Studio after installing?\nThis may come in handy for users who want to run .bat on Studio's files.";
            else if (sender.Equals(openFlagEditor))
                msg = "Allows you to enable certain Roblox engine features before they are available.\nThis is for expert users only, and you should avoid using this if you don't know how to.";

            if (msg != null)
                MessageBox.Show(msg, "Information about the \"" + controller.AccessibleName + "\" " + sender.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

            e.Handled = true;
        }

        public Launcher(params string[] mainArgs)
        {
            if (mainArgs.Length > 0)
                args = mainArgs;

            InitializeComponent();
        }
    }
}