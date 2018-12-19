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

        private static RegistryKey explRegistry = Program.GetSubKey("ExplorerIcons");
        private static RegistryKey iconRegistry = Program.GetSubKey(explRegistry, "EnabledIcons");
        private static RegistryKey infoRegistry = Program.GetSubKey(explRegistry, "ClassImagesInfo");

        private static Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);
        private static Color darkColor = Color.FromArgb(44, 44, 44);

        private delegate void WindowStateDelegator(FormWindowState windowState);
        private delegate void ButtonColorDelegator(Button button, Color newColor);
        private delegate void CheckBoxDelegator(CheckBox checkBox, bool setChecked);
        private delegate void StatusDelegator(Label label, string newText, Color newColor);

        private Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();

        private List<Image> iconLookup = new List<Image>();
        private List<Button> buttonLookup = new List<Button>();

        private string branch;
        private int selectedIndex = 0;
        private bool darkTheme = false;
        private bool showModifiedIcons = false;

        private FileSystemWatcher iconWatcher;
        private Timer statusUpdateTimer;
        private int lastStatusUpdate = -1;
        
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

        private static void updateExplorerIcons(string studioPath)
        {
            // Windows-1252 encoding is required to correctly read the exe.
            Encoding WINDOWS_1252 = Encoding.GetEncoding(1252);

            string studioBin = File.ReadAllText(studioPath, WINDOWS_1252);
            int pos = studioBin.Length / 2;

            Image icons = null;
            int memoryOffset = 0;
            int memorySize = 0;

            while (true)
            {
                int begin = studioBin.IndexOf("PNG\r\n\x1a\n", pos);

                if (begin >= 0)
                {
                    int ihdr = studioBin.IndexOf("IHDR", begin);

                    if ((ihdr - begin) <= 16)
                    {
                        int iend = studioBin.IndexOf("IEND", ihdr);
                        if (iend >= 0)
                        {
                            string pngFile = studioBin.Substring(begin - 1, (iend + 10) - begin);
                            byte[] pngBuffer = WINDOWS_1252.GetBytes(pngFile);

                            try
                            {
                                Image image;
                                using (MemoryStream stream = new MemoryStream(pngBuffer))
                                    image = Image.FromStream(stream);

                                if (image.Height == iconSize && image.Width % iconSize == 0)
                                {
                                    if (icons == null || image.Width > icons.Width)
                                    {
                                        icons = image;
                                        memoryOffset = begin - 1;
                                        memorySize = pngFile.Length;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error while processing image: " + e.Message);
                            }

                            pos = iend + 10;
                        }
                    }
                    else
                    {
                        // This was probably some random png header, but the IHDR chunk might be real
                        // See if we can jump behind it slightly and run into a proper file.
                        pos = Math.Max(pos, ihdr - 16);
                    }
                }
                else
                {
                    break;
                }
            }

            if (icons == null)
                throw new InvalidDataException("Failed to locate the explorer icon spritesheet in RobloxStudioBeta.exe!");

            string explorerDir = getExplorerIconDir();
            string classImages = Path.Combine(explorerDir, "__classImageRef.png");

            FileInfo fileInfo = new FileInfo(classImages);

            // If the file exists already, unlock it.
            if (fileInfo.Exists)
                fileInfo.Attributes = FileAttributes.Normal;

            // Write the updated icons file.
            icons.Save(classImages);
            icons.Dispose();

            // Lock the file so it isn't tampered with. This file is used as a
            // reference for the original icons in the sprite sheet.
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;

            // Update the registry with some information about the sprite sheet.
            infoRegistry.SetValue("SourceLocation", classImages);
            infoRegistry.SetValue("MemoryOffset", memoryOffset);
            infoRegistry.SetValue("MemorySize", memorySize);
        }

        private static Image getExplorerIcons()
        {
            string currentVersion = Program.GetRegistryString("BuildVersion");
            string patchVersion = Program.GetRegistryString(explRegistry, "LastPatchVersion");
        
            if (currentVersion != patchVersion)
            {
                string studioPath = RobloxInstaller.GetStudioPath();
                updateExplorerIcons(studioPath);

                explRegistry.SetValue("LastPatchVersion", currentVersion);
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

        private static long measureImageSize(Image image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return stream.Length;
            }
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

            bool result = true;

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    if (compA.GetPixel(x, y) != compB.GetPixel(x, y))
                    {
                        result = false;
                        break;
                    }
                }
            }

            compA.Dispose();
            compB.Dispose();

            return result;
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

        private void setCheckBoxCheck(CheckBox checkBox, bool check)
        {
            if (checkBox.InvokeRequired)
            {
                CheckBoxDelegator checkDelegator = new CheckBoxDelegator(setCheckBoxCheck);
                checkBox.Invoke(checkDelegator, checkBox, check);
            }
            else
            {
                if (checkBox.Checked != check)
                {
                    checkBox.Checked = check;
                }
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
            button.BackColor = (darkTheme ? Color.ForestGreen : Color.FromArgb(220, 255, 220));
            
            Task reset = Task.Run(async () =>
            {
                await Task.Delay(100);

                Color resetColor = (darkTheme ? darkColor : Color.White);
                setButtonColor(button, resetColor);
            });
        }

        private void updateStatus(object sender = null, EventArgs e = null)
        {
            int now = DateTime.Now.Second;

            if (now == lastStatusUpdate)
                return;

            lastStatusUpdate = now;

            // Update the memory status
            Image mainIcons = getExplorerIcons();
            Image patchIcons = getPatchedExplorerIcons();
            
            long mainLen = measureImageSize(mainIcons);
            long patchLen = measureImageSize(patchIcons);

            string available = (patchLen > mainLen ? "over" : "free");

            modifyStatusLabel
            (
                memoryStatus,
                "Memory Budget: " + patchLen + '/' + mainLen + " bytes used (" + Math.Abs(mainLen - patchLen) + " " + available + "!)",
                patchLen > mainLen ? Color.Red : Color.Green
            );

            // Update the studio status
            bool runningStudio = isRobloxStudioRunning();

            modifyStatusLabel
            (
                studioStatus,
                "Studio Status: " + (runningStudio ? "" : "Not") + " Running.",
                runningStudio ? Color.Red : Color.Green
            );

            // Update the error details.
            Color detailColor = Color.Red;
            string detailMsg = "";

            if (patchLen > mainLen)
            {
                detailMsg = "The size of the new explorer icon spritesheet exceeds\n"
                          + " the original file's size. Try to optimize some icons!";
            }
            else if (runningStudio)
            {
                detailMsg = "The explorer icon spritesheet cannot be updated\n"
                          + "while Roblox Studio is running!";
            }
            else
            {
                detailColor = Color.Green;
                detailMsg = "All set! Your icons will be patched in once you launch\n"
                          + "Roblox Studio from the Mod Manager!";
            }

            modifyStatusLabel(errors, detailMsg, detailColor);
        }

        private void updateStatusNow()
        {
            lastStatusUpdate = -1;
            updateStatus();
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

        private static void hideIconFile(int index)
        {
            try
            {
                string iconPath = getExplorerIconPath(index);
                if (File.Exists(iconPath))
                {
                    File.SetAttributes(iconPath, File.GetAttributes(iconPath) | FileAttributes.Hidden);
                }
            }
            catch
            {
                Console.WriteLine("Could not hide icon {0} at this time.", index);
            }
        }

        private static void showIconFile(int index)
        {
            try
            {
                string iconPath = getExplorerIconPath(index);
                if (File.Exists(iconPath))
                {
                    File.SetAttributes(iconPath, File.GetAttributes(iconPath) & ~FileAttributes.Hidden);
                }
            }
            catch
            {
                Console.WriteLine("Could not unhide icon {0} at this time.", index);
            }
        }

        private void setIconOverridden(int index, bool enabled)
        {
            if (index < 0)
                return;

            bool iconChanged = true;

            if (enabled)
            {
                showIconFile(index);
            }
            else
            {
                Image current = getIconForIndex(index);
                Image original = iconLookup[index];

                if (iconsAreEqual(original, current))
                {
                    iconChanged = false;
                    hideIconFile(index);
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
            selectedIcon.BackColor = darkTheme ? darkColor : Color.White;
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

            updateStatusNow();
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

                updateStatusNow();
            }
            else
            {
                int index = resolveIndexForFile(e.Name);
                setIconOverridden(index, true);
                updateStatus();
            }
        }

        private void ExplorerIconEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            EventHandler iconBtnClicked = new EventHandler(onIconBtnClicked);
            string studioPath = RobloxInstaller.GetStudioPath();

            bool.TryParse(Program.GetRegistryString(explRegistry, "ShowModifiedIcons"), out showModifiedIcons);
            showModified.Checked = showModifiedIcons;

            bool.TryParse(Program.GetRegistryString(explRegistry, "DarkTheme"), out darkTheme);
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

                    using (Graphics graphics = Graphics.FromImage(icon))
                        graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                    Button iconBtn = new Button();
                    iconBtn.BackColor = darkTheme ? darkColor : Color.White;
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

            statusUpdateTimer = new Timer();
            statusUpdateTimer.Tick += new EventHandler(updateStatus);
            statusUpdateTimer.Interval = 1000;
            statusUpdateTimer.Start();

            updateStatus();

            Enabled = true;
            UseWaitCursor = false;
        }

        public static async Task<bool> PatchExplorerIcons()
        {
            bool success = false;

            try
            {
                // The procedure for grabbing the explorer icons
                // *can* be expensive, so run it in a task.

                Image original = await Task.Factory.StartNew(getExplorerIcons);
                Image patched = getPatchedExplorerIcons();

                long oldSize = measureImageSize(original);
                long newSize = measureImageSize(patched);

                if (newSize <= oldSize && !isRobloxStudioRunning())
                {
                    string studioPath = RobloxInstaller.GetStudioPath();
                    int memoryOffset = (int)infoRegistry.GetValue("MemoryOffset");

                    using (FileStream studio = File.OpenWrite(studioPath))
                    {
                        studio.Position = memoryOffset;
                        patched.Save(studio, ImageFormat.Png);
                    }

                    success = true;
                }
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
                explRegistry.SetValue("ShowModifiedIcons", showModifiedIcons);
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
            explRegistry.SetValue("DarkTheme", darkTheme);
            themeSwitcher.Text = "Theme: " + (darkTheme ? "Dark" : "Light");
            
            Color color = darkTheme ? darkColor : Color.White;
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