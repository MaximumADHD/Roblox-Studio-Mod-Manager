using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobloxStudioModManager
{
    public partial class ExplorerIconEditor : Form
    {
        private string branch;
        private static Rectangle iconRect = new Rectangle(0, 0, 16, 16);

        private struct ExplorerIconInfo
        {
            public Image SpriteSheet;
            public int MemoryOffset;
        }

        public ExplorerIconEditor(string _branch)
        {
            InitializeComponent();
            branch = _branch;
        }

        private static ExplorerIconInfo GetExplorerIcons(string studioPath)
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

                            if (image.Height == 16 || image.Width % 16 != 0)
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
            info.SpriteSheet = icons;
            info.MemoryOffset = memoryOffset;

            return info;
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

            int numIcons = explorerIcons.Width / 16;
            SuspendLayout();

            for (int i = 0; i < numIcons; i++)
            {
                Bitmap icon = new Bitmap(16, 16);
                Rectangle srcRect = new Rectangle(i * 16, 0, 16, 16);

                using (Graphics graphics = Graphics.FromImage(icon))
                    graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                Button iconBtn = new Button();
                iconBtn.BackgroundImage = icon;
                iconBtn.BackgroundImageLayout = ImageLayout.Zoom;
                iconBtn.Size = new Size(24, 24);

                iconContainer.Controls.Add(iconBtn);
            }

            ResumeLayout();
            Enabled = true;
            UseWaitCursor = false;
        }
    }
}
