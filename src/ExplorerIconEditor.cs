using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class ExplorerIconEditor : Form
    {
        private const int iconSize = 16;
        private const string iconPrefix = "explorer-icon-";

        private static RegistryKey explRegistry = Program.GetSubKey(Program.ModManagerRegistry, "ExplorerIcons");
        private static RegistryKey iconRegistry = Program.GetSubKey(explRegistry, "EnabledIcons");
        private static RegistryKey infoRegistry = Program.GetSubKey(explRegistry, "ClassImagesInfo");

        private static Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);
        private static Color darkColor = Color.FromArgb(44, 44, 44);

        private string branch;
        private bool darkTheme = false;
        private int selectedIndex = 0;
        
        private List<Image> iconLookup = new List<Image>();
        private Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();
        private FileSystemWatcher watcher;

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
            string studioBin = File.ReadAllText(studioPath, Encoding.Default);
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
                            byte[] pngBuffer = Encoding.Default.GetBytes(pngFile);

                            Image image;
                            using (MemoryStream stream = new MemoryStream(pngBuffer))
                                image = Image.FromStream(stream);

                            if (image.Height == iconSize || image.Width % iconSize != 0)
                            {
                                if (icons == null || image.Width > icons.Width)
                                {
                                    icons = image;
                                    memoryOffset = begin - 1;
                                    memorySize = pngFile.Length;
                                }
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

            string studioDir = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioDir, "RobloxStudioBeta.exe");

            if (currentVersion != patchVersion)
            {
                updateExplorerIcons(studioPath);
                explRegistry.SetValue("LastPatchVersion", currentVersion);
            }

            string imagePath = Program.GetRegistryString(infoRegistry, "SourceLocation");
            return Image.FromFile(imagePath);
        }

        private Image getPatchedExplorerIcons()
        {
            string studioBin = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioBin, "RobloxStudioBeta.exe");

            Image explorerIcons = getExplorerIcons();
            Image newExplorerIcons = new Bitmap(explorerIcons.Width, explorerIcons.Height);

            int memorySize = (int)infoRegistry.GetValue("MemorySize");
            int memoryOffset = (int)infoRegistry.GetValue("MemoryOffset");

            Graphics explorerGraphics = Graphics.FromImage(newExplorerIcons);
            int numIcons = explorerIcons.Width / iconSize;

            bool patchedAny = false;

            for (int i = 0; i < numIcons; i++)
            {
                if (getIconOverridden(i))
                {
                    string filePath = getExplorerIconPath(i);
                    if (File.Exists(filePath))
                    {
                        using (Image icon = Image.FromFile(filePath))
                        {
                            Rectangle srcRect = new Rectangle(0, 0, icon.Width, icon.Height);
                            Rectangle destRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);
                            explorerGraphics.DrawImage(icon, destRect, srcRect, GraphicsUnit.Pixel);
                            patchedAny = true;
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

        private static bool isRobloxStudioRunning()
        {
            var studioProcs = Process.GetProcessesByName("RobloxStudioBeta.exe");
            return studioProcs.Length > 0;
        }

        private void updateStatus()
        {
            Image mainIcons = getExplorerIcons();
            Image patchIcons = getPatchedExplorerIcons();

            long mainLen = measureImageSize(mainIcons);
            long patchLen = measureImageSize(patchIcons);
            
            memoryStatus.Text = "Memory Budget: " + patchLen + '/' + mainLen + " bytes used, " + Math.Max(0, mainLen - patchLen) + " free.";
            memoryStatus.ForeColor = (patchLen > mainLen ? Color.Red : Color.Green);


            bool runningStudio = isRobloxStudioRunning();
            studioStatus.Text = "Studio Status: " + (runningStudio ? "" : "Not") + " running";
            studioStatus.ForeColor = (runningStudio ? Color.Red : Color.Green);

            if (patchLen > mainLen)
            {
                errors.Text = "The size of the new explorer icon spritesheet exceeds\n"
                            + " the original file's size. Try to optimize some icons!";

                errors.ForeColor = Color.Red;
            }
            else if (runningStudio)
            {
                errors.Text = "The explorer icon spritesheet cannot be updated\n"
                            + "while Roblox Studio is running!";

                errors.ForeColor = Color.Red;
            }
            else
            {
                errors.Text = "All good to go! Your icons will be patched in once\n"
                            + "you launch Roblox Studio from the mod manager!";

                errors.ForeColor = Color.Green;
            }


            mainIcons.Dispose();
            patchIcons.Dispose();
        }

        private static bool getIconOverridden(int index)
        {
            string key = iconPrefix + index;
            string value = Program.GetRegistryString(iconRegistry, key);

            bool result = false;
            bool.TryParse(value, out result);

            return result;
        }

        private void setIconOverridden(int index, bool enabled)
        {
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
        }

        private Image getIconForIndex(int index)
        {
            if (getIconOverridden(index))
            {
                string iconPath = getExplorerIconPath(index);

                // Copy the file to a memory stream so the image doesn't
                // hold a write lock on the icon file.
                MemoryStream stream = new MemoryStream();

                using (FileStream file = File.OpenRead(iconPath))
                    file.CopyTo(stream);

                Image result = Image.FromStream(stream);
                stream.Dispose();

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
            selectedIndex = index;

            bool enabled = getIconOverridden(selectedIndex);
            enableIconOverride.Checked = enabled;
            restoreOriginal.Enabled = enabled;
            editIcon.Enabled = enabled;
        }

        private void enableIconOverride_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = enableIconOverride.Checked;
            editIcon.Enabled = isChecked;
            restoreOriginal.Enabled = isChecked;

            setIconOverridden(selectedIndex, isChecked);
            selectedIcon.BackgroundImage = getIconForIndex(selectedIndex);

            updateStatus();
        }


        private void onIconBtnClicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = iconBtnIndex[button];
            setSelectedIndex(index);
        }

        private async void ExplorerIconEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            EventHandler iconBtnClicked = new EventHandler(onIconBtnClicked);

            string studioBin = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioBin, "RobloxStudioBeta.exe");

            string liveVersion = await RobloxInstaller.GetCurrentVersion(branch);
            await RobloxInstaller.BringUpToDate(branch, liveVersion, "The explorer icons may have been changed!");

            using (Image explorerIcons = getExplorerIcons())
            {
                int numIcons = explorerIcons.Width / iconSize;
                SuspendLayout();

                for (int i = 0; i < numIcons; i++)
                {
                    Bitmap icon = new Bitmap(iconSize, iconSize);
                    Rectangle srcRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);

                    using (Graphics graphics = Graphics.FromImage(icon))
                        graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                    Button iconBtn = new Button();
                    iconBtn.BackColor = Color.White;
                    iconBtn.BackgroundImage = icon;
                    iconBtn.BackgroundImageLayout = ImageLayout.Zoom;
                    iconBtn.Size = new Size(32, 32);
                    iconBtn.Click += iconBtnClicked;

                    iconLookup.Add(icon);
                    iconBtnIndex.Add(iconBtn, i);

                    iconContainer.Controls.Add(iconBtn);
                }

                setSelectedIndex(0);
                ResumeLayout();
            }

            string explorerIconDir = getExplorerIconDir();
            watcher = new FileSystemWatcher(explorerIconDir);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.png";

            Enabled = true;
            UseWaitCursor = false;

            updateStatus();
        }

        private void editIcon_Click(object sender, EventArgs e)
        {
            string explorerIconPath = getExplorerIconPath(selectedIndex);
            if (File.Exists(explorerIconPath))
            {
                Process.Start(explorerIconPath);
                WindowState = FormWindowState.Minimized;
            }
        }

        private void themeSwitcher_Click(object sender, EventArgs e)
        {
            darkTheme = !darkTheme;
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
                        selectedIcon.BackgroundImage = getIconForIndex(selectedIndex);
                        updateStatus();
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
    }
}