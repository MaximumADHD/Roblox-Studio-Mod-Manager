using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public partial class ClassIconEditor : Form
    {
        private const int iconSize = 16;
        private const int maxExtraIcons = 99;

        private const string iconPrefix = "explorer-icon-";
        private const string iconManifest = @"content\textures\ClassImages.PNG";
        private const string clientTracker = "https://raw.githubusercontent.com/MaximumADHD/Roblox-Client-Tracker";

        private static readonly RegistryKey explorerRegistry = Program.GetSubKey("ExplorerIcons");
        private static readonly RegistryKey manifestRegistry = Program.GetSubKey("FileManifest");

        private static readonly RegistryKey iconRegistry = explorerRegistry.GetSubKey("EnabledIcons");
        private static readonly RegistryKey infoRegistry = explorerRegistry.GetSubKey("ClassImagesInfo");

        private static readonly Color THEME_LIGHT_NORMAL = Color.White;
        private static readonly Color THEME_LIGHT_FLASH = Color.FromArgb(220, 255, 220);
        private static readonly Color THEME_DARK_NORMAL = Color.FromArgb(44, 44, 44);
        private static readonly Color THEME_DARK_FLASH = Color.ForestGreen;

        private delegate void AddControlDelegator(Control parent, Control child);
        private delegate void ButtonColorDelegator(Button button, Color newColor);

        private readonly List<Image> iconLookup = new List<Image>();
        private readonly List<Button> buttonLookup = new List<Button>();
        private readonly Dictionary<Button, int> iconBtnIndex = new Dictionary<Button, int>();

        private int selectedIndex;
        private bool darkTheme = false;
        private bool showModifiedIcons = false;
        private bool initializedExtraSlots = false;

        private static int numIcons = 0;
        private FileSystemWatcher iconWatcher;

        public ClassIconEditor()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            if (disposing && iconWatcher != null)
                iconWatcher.Dispose();

            base.Dispose(disposing);
        }

        private static string getExplorerIconDir()
        {
            string studioBin = StudioBootstrapper.GetStudioDirectory();
            string explorerBin = Path.Combine(studioBin, "ExplorerIcons");

            if (!Directory.Exists(explorerBin))
                Directory.CreateDirectory(explorerBin);

            return explorerBin;
        }

        private static string getExplorerIconPath(int index)
        {
            return Path.Combine(getExplorerIconDir(), iconPrefix + index + ".png");
        }

        private static void UpdateExplorerIcons(string studioDir)
        {
            // Find the class icons file.
            string iconPath = Path.Combine(studioDir, iconManifest);

            if (!File.Exists(iconPath))
                throw new Exception("Could not find " + iconManifest + "!");

            string explorerDir = getExplorerIconDir();
            string classImages = Path.Combine(explorerDir, "__classImageRef.png");

            FileInfo fileInfo = new FileInfo(classImages);

            // If the file exists already, unlock it.
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
            string manifestHash = manifestRegistry.GetString(iconManifest);
            string currentHash = infoRegistry.GetString("LastClassIconhash");

            if (currentHash != manifestHash)
            {
                string studioDir = StudioBootstrapper.GetStudioDirectory();
                UpdateExplorerIcons(studioDir);

                infoRegistry.SetValue("LastClassIconHash", manifestHash);
            }

            string imagePath = infoRegistry.GetString("SourceLocation");

            if (!File.Exists(imagePath))
            {
                // I tried to hide this file to prevent it from being deleted, but
                // several users still somehow kept doing it by accident.

                // As a result, I've added the textures folder to my client tracker
                // so this texture can be pulled remotely as a failsafe ¯\_(ツ)_/¯

                const string branch = "roblox";

                // string branch = Program
                //    .GetString("BuildBranch")
                //    .Replace("sitetest3", "sitetest2");

                using (var http = new WebClient())
                {
                    byte[] restore = http.DownloadData($"{clientTracker}/{branch}/textures/ClassImages.PNG");
                    File.WriteAllBytes(imagePath, restore);
                }
            }

            Image explorerIcons = Image.FromFile(imagePath);
            numIcons = explorerIcons.Width / iconSize;

            return explorerIcons;
        }

        private static int getExtraItemSlots()
        {
            string slots = explorerRegistry.GetValue("ExtraItemSlots") as string;

            if (int.TryParse(slots, out int value))
                value = Math.Max(0, Math.Min(99, value));

            return value;
        }

        private static Image getPatchedExplorerIcons()
        {
            int extraSlots = getExtraItemSlots();
            Image explorerIcons = getExplorerIcons();

            int patchWidth = explorerIcons.Width + (extraSlots * iconSize);
            Image newExplorerIcons = new Bitmap(patchWidth, explorerIcons.Height);

            Graphics explorerGraphics = Graphics.FromImage(newExplorerIcons);
            numIcons = explorerIcons.Width / iconSize;

            bool patchedAny = false;

            for (int i = 0; i < numIcons + extraSlots; i++)
            {
                if (hasIconOverride(i))
                {
                    string filePath = getExplorerIconPath(i);

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            using (var icon = Image.FromFile(filePath))
                            {
                                Rectangle srcRect = new Rectangle(0, 0, icon.Width, icon.Height);
                                Rectangle destRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);

                                explorerGraphics.DrawImage(icon, destRect, srcRect, GraphicsUnit.Pixel);
                                patchedAny = true;
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine($"Error while trying to load {filePath}");
                        }
                    }
                }
                else
                {
                    int captureIndex = (i < numIcons ? i : 0);

                    Rectangle capture = new Rectangle(captureIndex * iconSize, 0, iconSize, iconSize);
                    Rectangle destination = new Rectangle(i * iconSize, 0, iconSize, iconSize);

                    explorerGraphics.DrawImage(explorerIcons, destination, capture, GraphicsUnit.Pixel);
                }
            }

            explorerGraphics.Dispose();

            if (!patchedAny)
                newExplorerIcons = explorerIcons;

            return newExplorerIcons;
        }

        private static bool IconsAreEqual(Image a, Image b)
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

        private int resolveIndexForFile(string fileName)
        {
            int result = -1;
            bool success = false;

            if (fileName != iconPrefix + ".png" && fileName.StartsWith(iconPrefix, Program.StringFormat) && fileName.EndsWith(".png", Program.StringFormat))
            {
                string value = fileName.Substring(iconPrefix.Length, fileName.Length - iconPrefix.Length - 4);
                success = int.TryParse(value, out result);
            }

            if (!success || result >= numIcons + maxExtraIcons)
                // Out of range
                result = -1;
            else if (result >= numIcons)
                // Make sure these icon slots are allocated.
                itemSlots.Value = Math.Max(getExtraItemSlots(), result - numIcons + 1);

            return result;
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
                await Task
                    .Delay(100)
                    .ConfigureAwait(true);

                Color resetColor = (darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL);
                setButtonColor(button, resetColor);
            });
        }

        private static bool hasIconOverride(int index)
        {
            string key = iconPrefix + index;
            string value = iconRegistry.GetString(key);

            if (bool.TryParse(value, out bool result) && result)
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
            catch (UnauthorizedAccessException)
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

                if (IconsAreEqual(original, current))
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

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        string iconPath = getExplorerIconPath(index);

                        using (FileStream file = File.OpenRead(iconPath))
                            file.CopyTo(stream);

                        result = Image.FromStream(stream);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        result = iconLookup[index];
                    }
                }

                return result;
            }
            else
            {
                if (index >= iconLookup.Count)
                    index = 0;

                return iconLookup[index];
            }
        }

        private void setSelectedIndex(int index)
        {
            bool enabled = hasIconOverride(index);

            selectedIcon.BackgroundImage = getIconForIndex(index);
            selectedIcon.BackColor = darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL;
            selectedIndex = index;

            editIcon.Text = "Edit Icon " + index;
            enableIconOverride.Checked = enabled;
            restoreOriginal.Enabled = enabled;
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
                WindowState = FormWindowState.Normal;
            }
            else
            {
                int index = resolveIndexForFile(e.Name);
                setIconOverridden(index, true);
            }
        }

        private FileSystemEventHandler safeFileEventHandler(FileSystemEventHandler handler)
        {
            return new FileSystemEventHandler((sender, e) => Invoke(handler, sender, e));
        }

        private Button createIconButton(EventHandler clickEvent)
        {
            Button iconBtn = new Button
            {
                BackColor = darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL,
                BackgroundImageLayout = ImageLayout.Zoom,
                Size = new Size(48, 48)
            };

            iconBtn.Click += clickEvent;

            return iconBtn;
        }

        private void AddControlAcrossThread(Control parent, Control child)
        {
            if (parent.InvokeRequired)
            {
                var addControl = new AddControlDelegator(AddControlAcrossThread);
                parent.Invoke(addControl, parent, child);
            }
            else
            {
                parent.Controls.Add(child);
            }
        }

        private async void ClassIconEditor_Load(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;

            EventHandler iconBtnClicked = new EventHandler(onIconBtnClicked);
            string studioPath = StudioBootstrapper.GetStudioPath();

            showModifiedIcons = explorerRegistry.GetBool("ShowModifiedIcons");
            darkTheme = explorerRegistry.GetBool("DarkTheme");

            showModified.Checked = showModifiedIcons;
            themeSwitcher.Text = "Theme: " + (darkTheme ? "Dark" : "Light");

            selectedIcon.BackColor = (darkTheme ? THEME_DARK_NORMAL : THEME_LIGHT_NORMAL);
            selectedIcon.Refresh();

            int extraSlots = getExtraItemSlots();
            Image defaultIcon = null;

            SuspendLayout();

            var load = Task.Run(() =>
            {
                Image explorerIcons = getExplorerIcons();

                // Load Main Icons
                for (int i = 0; i < numIcons; i++)
                {
                    Button iconBtn = createIconButton(iconBtnClicked);

                    Bitmap icon = new Bitmap(iconSize, iconSize);
                    iconLookup.Add(icon);

                    Rectangle srcRect = new Rectangle(i * iconSize, 0, iconSize, iconSize);
                    Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);

                    using (Graphics graphics = Graphics.FromImage(icon))
                        graphics.DrawImage(explorerIcons, iconRect, srcRect, GraphicsUnit.Pixel);

                    if (defaultIcon == null)
                        defaultIcon = icon;

                    buttonLookup.Add(iconBtn);
                    iconBtnIndex.Add(iconBtn, i);

                    if (showModifiedIcons)
                        iconBtn.BackgroundImage = getIconForIndex(i);
                    else
                        iconBtn.BackgroundImage = icon;

                    AddControlAcrossThread(iconContainer, iconBtn);
                }

                // Load Extra Slots
                for (int i = 0; i < maxExtraIcons; i++)
                {
                    int slot = numIcons + i;

                    Button iconBtn = createIconButton(iconBtnClicked);
                    iconBtn.Visible = (i < extraSlots);

                    string fileName = getExplorerIconPath(slot);

                    if (i < extraSlots && File.Exists(fileName))
                    {
                        Image icon = getIconForIndex(slot);
                        iconBtn.BackgroundImage = icon;
                    }

                    iconLookup.Add(defaultIcon);
                    buttonLookup.Add(iconBtn);
                    iconBtnIndex.Add(iconBtn, slot);

                    AddControlAcrossThread(iconContainer, iconBtn);
                }

                explorerIcons.Dispose();
            });

            await load.ConfigureAwait(true);

            setSelectedIndex(0);
            ResumeLayout();

            itemSlots.Value = extraSlots;
            header.Text = "Select Icon";

            iconWatcher = new FileSystemWatcher(getExplorerIconDir())
            {
                Filter = "*.png",
                EnableRaisingEvents = true
            };

            iconWatcher.Created += safeFileEventHandler(onFileCreated);
            iconWatcher.Changed += safeFileEventHandler(onFileChanged);
            iconWatcher.Deleted += safeFileEventHandler(onFileDeleted);

            Enabled = true;
            UseWaitCursor = false;
        }

        public static async Task PatchExplorerIcons()
        {
            string studioDir = StudioBootstrapper.GetStudioDirectory();
            string iconPath = Path.Combine(studioDir, iconManifest);

            var getPatched = Task.Run(() => getPatchedExplorerIcons());
            Image patched = await getPatched.ConfigureAwait(true);

            patched.Save(iconPath);
        }

        private void itemSlots_ValueChanged(object sender, EventArgs e)
        {
            int oldSlots = initializedExtraSlots ? getExtraItemSlots() : 0;
            int extraSlots = (int)itemSlots.Value;

            for (int i = oldSlots; i < extraSlots; i++)
            {
                int slot = numIcons + i;

                Button btn = buttonLookup[slot];
                Image icon = getIconForIndex(slot);

                btn.BackgroundImage = icon;
                btn.Visible = true;
            }

            for (int i = oldSlots - 1; i >= extraSlots; i--)
            {
                int slot = numIcons + i;
                Button btn = buttonLookup[slot];
                btn.Visible = false;
            }

            int scrollToIndex = numIcons + extraSlots - 1;
            Button viewBtn = buttonLookup[scrollToIndex];

            if (initializedExtraSlots)
                iconContainer.ScrollControlIntoView(viewBtn);

            if (selectedIndex >= (numIcons + extraSlots))
                setSelectedIndex(scrollToIndex);

            explorerRegistry.SetValue("ExtraItemSlots", extraSlots.ToString(Program.NumberFormat));
            initializedExtraSlots = true;
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
                    catch (IOException)
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

        private void itemSlots_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
