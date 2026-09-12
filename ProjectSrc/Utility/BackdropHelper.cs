using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace RobloxStudioModManager
{
    internal static class BackdropHelper
    {
        public static bool IsWindows11 => Environment.OSVersion.Version.Build >= 22000;

        // Applies a standard backdrop to a window.
        public static void ApplyBackdrop(this Window window)
        {
            window.SystemBackdrop = IsWindows11
                ? (SystemBackdrop)new MicaBackdrop()
                : new DesktopAcrylicBackdrop();
        }
    }
}
