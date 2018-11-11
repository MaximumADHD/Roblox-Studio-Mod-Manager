using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class ExplorerIconEditor : Form
    {
        private static RegistryKey expRegistry = Program.GetSubKey(Program.ModManagerRegistry, "ExplorerIcons");

        private static int iconSize = 16;
        private static Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);

        private string branch;
        private int selectedIndex;

        private List<Image> iconLookup = new List<Image>();
        private Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();
        
        public struct ExplorerIconInfo
        {
            public string Source;
            public int MemoryOffset;
            public Image SpriteSheet;
        }

        public ExplorerIconEditor(string _branch)
        {
            InitializeComponent();
            branch = _branch;
        }

        private string getExplorerIconPath()
        {
            string studioBin = RobloxInstaller.GetStudioDirectory();
            string explorerBin = Path.Combine(studioBin, "ExplorerIcons");

            if (!Directory.Exists(explorerBin))
                Directory.CreateDirectory(explorerBin);

            return Path.Combine(explorerBin, "explorer-icon-" + selectedIndex + ".png");
        }

        public static ExplorerIconInfo GetExplorerIcons(string studioPath)
        {
            string studioBin = File.ReadAllText(studioPath, Encoding.Default);
            int pos = studioBin.Length / 2;

            Image icons = null;
            int memoryOffset = 0;

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

            ExplorerIconInfo info = new ExplorerIconInfo();
            info.Source = studioPath;
            info.SpriteSheet = icons;
            info.MemoryOffset = memoryOffset;

            return info;
        }

        private static bool PatchStudioIcons(string versionGuid)
        {
            // todo
            return false;
        }

        private void setSelectedIndex(int index)
        {
            Image icon = iconLookup[index];
            selectedIcon.BackgroundImage = icon;
            selectedIndex = index;

            bool enabled = (Program.GetRegistryString(expRegistry, "explorer-icon-" + selectedIndex) == "True");
            enableIconOverride.Checked = enabled;
            editIcon.Enabled = enabled;
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

            string studioBin = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioBin, "RobloxStudioBeta.exe");

            string liveVersion = await RobloxInstaller.GetCurrentVersion(branch);
            await RobloxInstaller.BringUpToDate(branch, liveVersion, "The explorer icons may have been changed!");

            ExplorerIconInfo explorerInfo = GetExplorerIcons(studioPath);
            Image explorerIcons = explorerInfo.SpriteSheet;

            EventHandler iconBtnClicked = new EventHandler(onIconBtnClicked);

            int numIcons = explorerIcons.Width / iconSize;
            SuspendLayout();

            for (int i = 0; i < numIcons; i++)
            {
                Bitmap icon = new Bitmap(iconSize, iconSize);
                Rectangle srcRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);

                using (Graphics graphics = Graphics.FromImage(icon))
                    graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                Button iconBtn = new Button();
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

            Enabled = true;
            UseWaitCursor = false;
        }

        private void enableIconOverride_CheckedChanged(object sender, EventArgs e)
        {
            string explorerIconPath = getExplorerIconPath();
            bool isChecked = enableIconOverride.Checked;

            if (isChecked && !File.Exists(explorerIconPath))
            {
                Image icon = iconLookup[selectedIndex];
                icon.Save(explorerIconPath);
            }

            editIcon.Enabled = isChecked;
            expRegistry.SetValue("explorer-icon-" + selectedIndex, isChecked);
        }

        private void editIcon_Click(object sender, EventArgs e)
        {
            string explorerIconPath = getExplorerIconPath();
            if (File.Exists(explorerIconPath))
            {
                Process.Start(explorerIconPath);
            }
        }
    }
}