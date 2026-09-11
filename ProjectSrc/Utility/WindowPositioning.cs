using System;
using System.Runtime.InteropServices;

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Windows.Graphics;

namespace RobloxStudioModManager
{
    // This class allows windows to define an explicit size,
    // replicate the WinForms' old StartPosition default behavior,
    // and achieve modal behavior by setting an owner property.
    internal static class WindowPositioning
    {
        private const int GWL_HWNDPARENT = -8;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        public static void ResizeAndCenter(AppWindow appWindow, int width, int height, AppWindow owner = null)
        {
            appWindow.Resize(new SizeInt32(width, height));
            center(appWindow, owner);
        }

        // Same as above, but width/height describe the client (content) area
        // rather than the outer window.
        public static void ResizeClientAndCenter(AppWindow appWindow, int width, int height, AppWindow owner = null)
        {
            appWindow.ResizeClient(new SizeInt32(width, height));
            center(appWindow, owner);
        }

        private static void center(AppWindow appWindow, AppWindow owner)
        {
            RectInt32 bounds;

            if (owner != null)
            {
                bounds = new RectInt32(owner.Position.X, owner.Position.Y, owner.Size.Width, owner.Size.Height);
            }
            else
            {
                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);

                if (displayArea == null)
                    return;

                bounds = displayArea.WorkArea;
            }

            var x = bounds.X + (bounds.Width - appWindow.Size.Width) / 2;
            var y = bounds.Y + (bounds.Height - appWindow.Size.Height) / 2;

            appWindow.Move(new PointInt32(x, y));
        }

        // Marks the given window as owned by the owner.
        // Keeping it tied to the owner, and thus allowing the window to behave as modal.
        public static void SetOwner(Window window, Window owner)
        {
            var hwnd = Win32Interop.GetWindowFromWindowId(window.AppWindow.Id);
            var ownerHwnd = Win32Interop.GetWindowFromWindowId(owner.AppWindow.Id);
            SetWindowLongPtr(hwnd, GWL_HWNDPARENT, ownerHwnd);
        }

        // Disables/re-enables a window's input since WinUI has no ShowDialog(),
        // so this is how a dialog window blocks interaction with its owner
        // while it's open.
        public static void SetEnabled(Window window, bool enabled)
        {
            var hwnd = Win32Interop.GetWindowFromWindowId(window.AppWindow.Id);
            EnableWindow(hwnd, enabled);
        }
    }
}
