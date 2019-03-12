using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class ExplorerIconEditor : Form
    {
        private const int iconSize = 16;
        private const string iconPrefix = "explorer-icon-";
        private const string iconManifest = @"content\textures\ClassImages.PNG";
        
        private static RegistryKey explorerRegistry = Program.GetSubKey("ExplorerIcons");
        private static RegistryKey manifestRegistry = Program.GetSubKey("FileManifest");

        private static RegistryKey iconRegistry = Program.GetSubKey(explorerRegistry, "EnabledIcons");
        private static RegistryKey infoRegistry = Program.GetSubKey(explorerRegistry, "ClassImagesInfo");

        private static Color THEME_LIGHT_NORMAL = Color.White;
        private static Color THEME_LIGHT_FLASH  = Color.FromArgb(220, 255, 220);
        private static Color THEME_DARK_NORMAL  = Color.FromArgb(44, 44, 44);
        private static Color THEME_DARK_FLASH   = Color.ForestGreen;

        private delegate void WindowStateDelegator(FormWindowState windowState);
        private delegate void ButtonColorDelegator(Button button, Color newColor);
        private delegate void StatusDelegator(Label label, string newText, Color newColor);

        private List<Image> iconLookup = new List<Image>();
        private List<Button> buttonLookup = new List<Button>();
        private Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();

        private string branch;
        private int selectedIndex;
        private bool darkTheme = false;
        private bool showModifiedIcons = false;
        private FileSystemWatcher iconWatcher;

        public ExplorerIconEditor(string _branch)
        {
            InitializeComponent();
            branch = _branch;
        }

        private static string getExplorerIconDir()
        {
            string studioBin = RobloxInstaller.GetStudioDirectory();
            string explorerBin = Path.Combine(studioBin, "ExplorerIcons");

            if (!Directory.Exists(explorerBin))
                Directory.CreateDirectory(explorerBin);

            return explorerBin;
        }

        private static string getExplorerIconPath(int index)
        {
            return Path.Combine(getExplorerIconDir(), iconPrefix + index + ".png");
        }

        private static void updateExplorerIcons(string studioDir)
        {
            // Find the class icons file.
            string iconPath = Path.Combine(studioDir, iconManifest);

            if (!File.Exists(iconPath))
                throw new Exception("Could not find " + iconManifest + "!");

            string explorerDir = getExplorerIconDir();
            string classImages = Path.Combine(explorerDir, "__classImageRef.png");

            FileInfo fileInfo = new FileInfo(classImages);

            // If the file exists already, delete it.
            if (fileInfo.Exists)
                fileInfo.Attributes = FileAttributes.Normal;

            // Write the updated icons file.
            File.Copy(iconPath, classImages, true);

            // Lock the file so it isn't tampered with. This file is used as a
            // reference for the original icons in the sprite sheet.
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;

            // Update the registry with some information about the sprite sheet.
            infoRegistry.SetValue("SourceLocation", classImages);
        }

        private static Image getExplorerIcons()
        {
            string manifestHash = Program.GetRegistryString(manifestRegistry, iconManifest);
            string currentHash = Program.GetRegistryString(infoRegistry, "LastClassIconHash");

            if (currentHash != manifestHash)
            {
                string studioDir = RobloxInstaller.GetStudioDirectory();
                updateExplorerIcons(studioDir);

                infoRegistry.SetValue("LastClassIconHash", manifestHash);
            }
            
            string imagePath = Program.GetRegistryString(infoRegistry, "SourceLocation");
            return Image.FromFile(imagePath);
        }

        private static Image getPatchedExplorerIcons()
        {
            Image explorerIcons = getExplorerIcons();
            Image newExplorerIcons = new Bitmap(explorerIcons.Width, explorerIcons.Height);

            int memorySize = (int)infoRegistry.GetValue("MemorySize");
            int memoryOffset = (int)infoRegistry.GetValue("MemoryOffset");

            Graphics explorerGraphics = Graphics.FromImage(newExplorerIcons);
            int numIcons = explorerIcons.Width / iconSize;

            bool patchedAny = false;

            for (int i = 0; i < numIcons; i++)
            {
                if (hasIconOverride(i))
                {
                    string filePath = getExplorerIconPath(i);

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            using (Image icon = Image.FromFile(filePath))
                            {
                                Rectangle srcRect = new Rectangle(0, 0, icon.Width, icon.Height);
                                Rectangle destRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);
                                explorerGraphics.DrawImage(icon, destRect, srcRect, GraphicsUnit.Pixel);
                                patchedAny = true;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Error while trying to load {0}", filePath);
                        }
                    }
                }
                else
                {
                    Rectangle capture = new Rectangle(i * iconSize, 0, iconSize, iconSize);
                    explorerGraphics.DrawImage(explorerIcons, capture, capture, GraphicsUnit.Pixel);
                }
            }

            explorerGraphics.Dispose();

            if (!patchedAny)
                newExplorerIcons = explorerIcons;

            return newExplorerIcons;
        }

        private static bool iconsAreEqual(Image a, Image b)
        {
            // Compare by literal object
            if (a == b)
                return true;

            // Compare by size
            if (a.Size != b.Size)
                return false;

            // Compare pixels
            Bitmap compA = new Bitmap(a);
            Bitmap compB = new Bitmap(b);

            bool iconsEqual = true;

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    if (compA.GetPixel(x, y) != compB.GetPixel(x, y))
                    {
                        iconsEqual = false;
                        break;
                    }
                }
            }

            compA.Dispose();
            compB.Dispose();

            return iconsEqual;
        }

        private static bool isRobloxStudioRunning()
        {
            var studioProcs = RobloxInstaller.GetRunningStudioProcesses();
            return studioProcs.Count > 0;
        }
        
        private static void modifyStatusLabel(Label label, string newText, Color newColor)
        {
            if (label.InvokeRequired)
            {
                StatusDelegator update = new StatusDelegator(modifyStatusLabel);
                label.Invoke(update, label, newText, newColor);
            }
            else
            {
                label.Text = newText;
                label.ForeColor = newColor;
                label.Refresh();
            }
        }

        private int resolveIndexForFile(string fileName)
        {
            int result = -1;

            if (fileName != iconPrefix + ".png" && fileName.StartsWith(iconPrefix) && fileName.EndsWith(".png"))
            {
                string value = fileName.Substring(iconPrefix.Length, fileName.Length - iconPrefix.Length - 4);
                int.TryParse(value, out result);
            }
            
            if (result > iconLookup.Count)
                result = -1; // out of range

            return result;
        }

        private void setFormState(FormWindowState state)
        {
            if (InvokeRequired)
            {
                WindowStateDelegator delegator = new WindowStateDelegator(setFormState);
                Invoke(delegator, state);
            }
            else
            {
                WindowState = state;
            }
        }

        private void setButtonColor(Button button, Color color)
        {
            if (button.InvokeRequired)
            {
                ButtonColorDelegator delegator = new ButtonColorDelegator(setButtonColor);
                button.Invoke(delegator, button, color);
            }
            else
            {
                button.BackColor = color;
            }
        }

        private void flashButton(Button button)
        {
            button.BackColor = (darkTheme ? THEME_DARK_FLASH : THEME_LIGHT_FLASH);
            
            Task reset = Task.Run(async () =>
            {
                await Task.Delay(100);

                Color resetColor = (darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL);
                setButtonColor(button, resetColor);
            });
        }

        private static bool hasIconOverride(int index)
        {
            string key = iconPrefix + index;
            string value = Program.GetRegistryString(iconRegistry, key);

            bool result = false;
            bool.TryParse(value, out result);

            if (result)
            {
                // Double check that the file still exists.
                string fileName = getExplorerIconPath(index);

                if (!File.Exists(fileName))
                {
                    result = false;
                    iconRegistry.DeleteValue(key, false);
                }
            }

            return result;
        }

        private static bool setIconAttributes(int index, Func<FileAttributes, FileAttributes> set)
        {
            try
            {
                string iconPath = getExplorerIconPath(index);

                if (File.Exists(iconPath))
                {
                    FileAttributes oldAttributes = File.GetAttributes(iconPath);
                    FileAttributes newAttributes = set(oldAttributes);

                    File.SetAttributes(iconPath, newAttributes);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void setIconOverridden(int index, bool enabled)
        {
            if (index < 0)
                return;

            bool iconChanged = true;

            if (enabled)
            {
                setIconAttributes(index, attributes => attributes & ~FileAttributes.Hidden);
            }
            else
            {
                Image current = getIconForIndex(index);
                Image original = iconLookup[index];

                if (iconsAreEqual(original, current))
                {
                    iconChanged = false;
                    setIconAttributes(index, attributes => attributes | FileAttributes.Hidden);
                }
            }

            string key = iconPrefix + index;
            iconRegistry.SetValue(key, enabled);

            if (enabled)
            {
                string explorerIconPath = getExplorerIconPath(index);

                if (!File.Exists(explorerIconPath))
                {
                    Image icon = iconLookup[index];
                    icon.Save(explorerIconPath);
                }
            }

            if (iconChanged && index == selectedIndex)
                selectedIcon.BackgroundImage = getIconForIndex(index);

            if (showModifiedIcons)
            {
                Button iconBtn = buttonLookup[index];
                Image icon = getIconForIndex(index);

                if (iconBtn.BackgroundImage != icon)
                {
                    iconBtn.BackgroundImage = icon;
                    flashButton(iconBtn);
                }
            }
        }

        private Image getIconForIndex(int index)
        {
            if (hasIconOverride(index))
            {
                Image result;

                using (MemoryStream stream = new MemoryStream())
                {
                    try
                    {
                        string iconPath = getExplorerIconPath(index);

                        using (FileStream file = File.OpenRead(iconPath))
                            file.CopyTo(stream);

                        result = Image.FromStream(stream);
                    }
                    catch
                    {
                        result = iconLookup[index];
                    }
                }

                return result;
            }
            else
            {
                return iconLookup[index];
            }
        }

        private void setSelectedIndex(int index)
        {
            Image icon = iconLookup[index];
            selectedIcon.BackgroundImage = getIconForIndex(index);
            selectedIcon.BackColor = darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL;
            selectedIndex = index;

            bool enabled = hasIconOverride(index);
            enableIconOverride.Checked = enabled;
            restoreOriginal.Enabled = enabled;

            editIcon.Text = "Edit Icon " + index;
        }
        
        private void enableIconOverride_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = enableIconOverride.Checked;
            restoreOriginal.Enabled = isChecked;

            setIconOverridden(selectedIndex, isChecked);
            selectedIcon.BackgroundImage = getIconForIndex(selectedIndex);
        }

        private void onIconBtnClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = iconBtnIndex[button];
            setSelectedIndex(index);
        }
        
        private void onFileCreated(object sender, FileSystemEventArgs e)
        {
            int index = resolveIndexForFile(e.Name);
            setIconOverridden(index, true);
        }

        private void onFileDeleted(object sender, FileSystemEventArgs e)
        {
            int index = resolveIndexForFile(e.Name);
            setIconOverridden(index, false);
        }

        private void onFileChanged(object sender, FileSystemEventArgs e)
        {
            string selectedPath = getExplorerIconPath(selectedIndex);

            if (e.FullPath == selectedPath && hasIconOverride(selectedIndex))
            {
                Image icon = getIconForIndex(selectedIndex);

                if (showModifiedIcons)
                {
                    Button button = buttonLookup[selectedIndex];
                    button.BackgroundImage = icon;
                }

                selectedIcon.BackgroundImage = icon;
                setFormState(FormWindowState.Normal);
            }
            else
            {
                int index = resolveIndexForFile(e.Name);
                setIconOverridden(index, true);
            }
        }

        private void ExplorerIconEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            EventHandler iconBtnClicked = new EventHandler(onIconBtnClicked);
            string studioPath = RobloxInstaller.GetStudioPath();

            bool.TryParse(Program.GetRegistryString(explorerRegistry, "ShowModifiedIcons"), out showModifiedIcons);
            showModified.Checked = showModifiedIcons;

            bool.TryParse(Program.GetRegistryString(explorerRegistry, "DarkTheme"), out darkTheme);
            themeSwitcher.Text = "Theme: " + (darkTheme ? "Dark" : "Light");

            using (Image explorerIcons = getExplorerIcons())
            {
                int numIcons = explorerIcons.Width / iconSize;
                SuspendLayout();

                for (int i = 0; i < numIcons; i++)
                {
                    Bitmap icon = new Bitmap(iconSize, iconSize);
                    iconLookup.Add(icon);

                    Rectangle srcRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);
                    Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);

                    using (Graphics graphics = Graphics.FromImage(icon))
                        graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                    Button iconBtn = new Button();
                    iconBtn.BackColor = darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL;
                    iconBtn.BackgroundImageLayout = ImageLayout.Zoom;
                    iconBtn.Size = new Size(32, 32);
                    iconBtn.Click += iconBtnClicked;
                    
                    iconBtnIndex.Add(iconBtn, i);
                    buttonLookup.Add(iconBtn);

                    if (showModifiedIcons)
                        iconBtn.BackgroundImage = getIconForIndex(i);
                    else
                        iconBtn.BackgroundImage = icon;

                    iconContainer.Controls.Add(iconBtn);
                }

                setSelectedIndex(0);
                ResumeLayout();
            }

            iconWatcher = new FileSystemWatcher(getExplorerIconDir());
            iconWatcher.Filter = "*.png";
            iconWatcher.EnableRaisingEvents = true;

            iconWatcher.Created += new FileSystemEventHandler(onFileCreated);
            iconWatcher.Changed += new FileSystemEventHandler(onFileChanged);
            iconWatcher.Deleted += new FileSystemEventHandler(onFileDeleted);

            Enabled = true;
            UseWaitCursor = false;
        }

        public static bool PatchExplorerIcons()
        {
            bool success = false;

            try
            {
                string studioDir = RobloxInstaller.GetStudioDirectory();
                string iconPath = Path.Combine(studioDir, iconManifest);

                Image patched = getPatchedExplorerIcons();
                patched.Save(iconPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while trying to patch the explorer icons: {0}", e.Message);
            }

            return success;
        }

        private void showModified_CheckedChanged(object sender, EventArgs e)
        {
            if (showModifiedIcons != showModified.Checked)
            {
                showModifiedIcons = showModified.Checked;
                explorerRegistry.SetValue("ShowModifiedIcons", showModifiedIcons);
            }

            foreach (Button button in iconBtnIndex.Keys)
            {
                int index = iconBtnIndex[button];
                Image icon;

                if (showModifiedIcons && hasIconOverride(index))
                    icon = getIconForIndex(index);
                else
                    icon = iconLookup[index];

                if (button.BackgroundImage != icon)
                {
                    button.BackgroundImage = icon;
                    flashButton(button);
                }
            }
        }

        private void editIcon_Click(object sender, EventArgs e)
        {
            string explorerIconPath = getExplorerIconPath(selectedIndex);
            enableIconOverride.Checked = true; // Make sure the override is enabled.

            if (File.Exists(explorerIconPath))
            {
                Process.Start(explorerIconPath);
                WindowState = FormWindowState.Minimized;
            }
        }

        private void themeSwitcher_Click(object sender, EventArgs e)
        {
            darkTheme = !darkTheme;
            explorerRegistry.SetValue("DarkTheme", darkTheme);
            themeSwitcher.Text = "Theme: " + (darkTheme ? "Dark" : "Light");
            
            Color color = darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL;
            SuspendLayout();

            foreach (Button button in iconBtnIndex.Keys)
                button.BackColor = color;

            selectedIcon.BackColor = color;
            ResumeLayout();
        }

        private void restoreOriginal_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show
            (
                "Restoring the original icon will reset the icon file\n" +
                "you have created. This cannot be undone.\n" +
                "Are you sure you would like to continue?",

                "Warning",

                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                string explorerIconPath = getExplorerIconPath(selectedIndex);

                if (File.Exists(explorerIconPath))
                {
                    try
                    {
                        File.Delete(explorerIconPath);
                        setIconOverridden(selectedIndex, true);
                    }
                    catch
                    {
                        MessageBox.Show
                        (
                            "Could not restore the original file!\n" +
                            "Check if you have the icon open in an editor.",

                            "Error",

                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void openIconFolder_Click(object sender, EventArgs e)
        {
            string explorerIconDir = getExplorerIconDir();
            Process.Start(explorerIconDir);
        }
    }
}