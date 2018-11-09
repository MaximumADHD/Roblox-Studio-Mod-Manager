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

        public ExplorerIconEditor(string _branch)
        {
            InitializeComponent();
            branch = _branch;
        }

        private static Image GetExplorerIcons(string studioPath)
        {
            string studioBin = File.ReadAllText(studioPath, Encoding.Default);
            int at = studioBin.Length / 2;

            Image icons = null;

            while (true)
            {
                int begin = studioBin.IndexOf("PNG\r\n\x1a\n", at);
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
                                if (icons == null || image.Width > icons.Width)
                                    icons = image;

                            at = iend + 10;
                        }
                    }
                    else
                    {
                        // This was probably some random png header, but the IHDR chunk might be real
                        // See if we can jump behind it slightly and run into a proper file.
                        at = Math.Max(at, ihdr - 16);
                    }
                }
                else
                {
                    break;
                }
            }

            return icons;
        }

        private async void ExplorerIconEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            string studioBin = RobloxInstaller.GetStudioDirectory();
            string studioPath = Path.Combine(studioBin, "RobloxStudioBeta.exe");

            string liveVersion = await RobloxInstaller.GetCurrentVersion(branch);
            await RobloxInstaller.BringUpToDate(branch, liveVersion, "The explorer icons may have been changed!");

            Image explorerIcons = GetExplorerIcons(studioPath);
            int numIcons = explorerIcons.Width / 16;

            string imagePath = Path.Combine(studioBin, "Images");
            Directory.CreateDirectory(imagePath);

            Rectangle iconRect = new Rectangle(0, 0, 16, 16);
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
