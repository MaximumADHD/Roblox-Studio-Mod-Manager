using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class ExplorerIconEditor : Form
    {
        private const int iconSize = 16;
        private const string iconPrefix = "explorer-icon-";

        private static RegistryKey expRegistry = Program.GetSubKey(Program.ModManagerRegistry, "ExplorerIcons");
        private static RegistryKey iconRegistry = Program.GetSubKey(expRegistry, "EnabledIcons");
        private static RegistryKey imgInfoRegistry = Program.GetSubKey(expRegistry, "ClassImagesInfo");

        private static Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);
        private static Color darkColor = Color.FromArgb(44, 44, 44);

        private string branch;
        private bool darkTheme = false;
        private int selectedIndex = 0;
        
        private List<Image> iconLookup = new List<Image>();
        private Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();

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
            imgInfoRegistry.SetValue("SourceLocation", classImages);
            imgInfoRegistry.SetValue("MemoryOffset", memoryOffset);
            imgInfoRegistry.SetValue("MemorySize", memorySize);
        }

        private static Image getExplorerIcons()
        {
            string currentVersion = Program.GetRegistryString("BuildVersion");
            string patchVersion = Program.GetRegistryString(expRegistry, "LastPatchVersion");

            string studioDir = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioDir, "RobloxStudioBeta.exe");

            if (currentVersion != patchVersion)
            {
                updateExplorerIcons(studioPath);
                expRegistry.SetValue("LastPatchVersion", currentVersion);
            }

            string imagePath = Program.GetRegistryString(imgInfoRegistry, "SourceLocation");
            return Image.FromFile(imagePath);
        }

        private static bool getIconOverriden(int index)
        {
            string key = iconPrefix + index;
            string value = Program.GetRegistryString(iconRegistry, key);

            bool result = false;
            bool.TryParse(value, out result);

            return result;
        }

        private void setIconOverriden(int index, bool enabled)
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
            if (getIconOverriden(index))
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

            bool enabled = getIconOverriden(selectedIndex);
            enableIconOverride.Checked = enabled;
            restoreOriginal.Enabled = enabled;
            editIcon.Enabled = enabled;
        }

        private void enableIconOverride_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = enableIconOverride.Checked;
            editIcon.Enabled = isChecked;
            restoreOriginal.Enabled = isChecked;

            setIconOverriden(selectedIndex, isChecked);
            selectedIcon.BackgroundImage = getIconForIndex(selectedIndex);
        }

        public static bool PatchStudioIcons(string versionGuid)
        {
            return true;
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
            
            Enabled = true;
            UseWaitCursor = false;
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
                        setIconOverriden(selectedIndex, true);
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