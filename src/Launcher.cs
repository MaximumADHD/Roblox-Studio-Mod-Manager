using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

        private void manageMods_Click(object sender, EventArgs e)
        {
            string modPath = getModPath();
            Process.Start(modPath);
        }

        private void editFVariables_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Editing FVariables can make Roblox Studio unstable.\nYou should only change them if you know what you're doing.\nAre you sure you would like to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                string dataBase = (string)dataBaseSelect.SelectedItem;
                FVariableEditor editor = new FVariableEditor(this, dataBase);
                editor.Show();
            }
        }

        private async void launchStudio_Click(object sender = null, EventArgs e = null)
        {
            Hide();
            string dataBase = (string)dataBaseSelect.SelectedItem;
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
                    metaScript = metaScript.Replace("%MOD_MANAGER_VERSION%", '"' + dataBase + '"'); // TODO: Make this something more generic?
                }

                string dir = Path.Combine(studioRoot, "BuiltInPlugins");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string metaScriptFile = Path.Combine(dir, "__rbxModManagerMetadata.lua");
                FileInfo info = new FileInfo(metaScriptFile);
                if (info.Exists)
                    info.Attributes = FileAttributes.Normal;

                File.WriteAllText(metaScriptFile, metaScript);

                // Make the file as readonly so that it (hopefully) won't be messed with.
                // I can't hide the file because Roblox Studio will ignore it.
                // If someone has the file open with write permissions, it will fail to write.
                
                info.Attributes = FileAttributes.ReadOnly;
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

            Process process;
            if (openStudioDirectory.Checked)
                process = Process.Start(studioRoot);
            else
                process = Process.Start(robloxStudioInfo);

            try
            {
                IntPtr handle = process.MainWindowHandle;
                Program.SetForegroundWindow(handle);
            }
            catch
            {
                Console.WriteLine("Can't bring this window to the foreground.");
            }

            Environment.Exit(0);
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            object registrySave = Program.ModManagerRegistry.GetValue("BuildDatabase");
            if (registrySave != null)
            {
                string build = registrySave as string;
                dataBaseSelect.SelectedIndex = dataBaseSelect.Items.IndexOf(build);
            }
            else
            {
                dataBaseSelect.SelectedIndex = 0;
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
            else if (sender.Equals(dataBaseSelect))
                msg = "Indicates which setup web-domain we should use to download Roblox Studio.\nThe gametest domains are prototype versions of ROBLOX Studio,\nthat are not available on the main site yet.";
            else if (sender.Equals(forceRebuild))
                msg = "Should we forcefully reinstall this version of the client, even if its already installed?\nThis can be used if you are experiencing a problem with launching Roblox Studio.";
            else if (sender.Equals(openStudioDirectory))
                msg = "Should we just open the directory of Roblox Studio after installing?\nThis may come in handy for users who want to run .bat on Studio's files.";
            else if (sender.Equals(editFVariables))
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

            if (mainArgs.Length == 1 && mainArgs[0].StartsWith("roblox-studio")) // If we were launched from a URI, don't show the directory-only option.
                openStudioDirectory.Visible = false;
        }
    }
}