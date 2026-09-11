using Microsoft.UI.Windowing;

using Windows.Graphics;

namespace RobloxStudioModManager
{
    // Shared by any window that needs an explicit size (WinUI's unconfigured
    // default window size doesn't match the original WinForms sizing) and
    // WinForms' old StartPosition = CenterScreen behavior, which WinUI has no
    // built-in equivalent for.
    internal static class WindowPositioning
    {
        public static void ResizeAndCenter(AppWindow appWindow, int width, int height)
        {
            appWindow.Resize(new SizeInt32(width, height));
            center(appWindow);
        }

        // Same as above, but width/height describe the client (content) area
        // rather than the outer window
        public static void ResizeClientAndCenter(AppWindow appWindow, int width, int height)
        {
            appWindow.ResizeClient(new SizeInt32(width, height));
            center(appWindow);
        }

        private static void center(AppWindow appWindow)
        {
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);

            if (displayArea == null)
                return;

            var x = displayArea.WorkArea.X + (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
            var y = displayArea.WorkArea.Y + (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;

            appWindow.Move(new PointInt32(x, y));
        }
    }
}
