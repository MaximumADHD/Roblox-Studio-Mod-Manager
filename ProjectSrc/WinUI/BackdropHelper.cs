using System;

namespace RobloxStudioModManager.WinUI
{
    internal static class BackdropHelper
    {
        // Windows 11 build numbers start at 22000.
        public static bool IsWindows11 => Environment.OSVersion.Version.Build >= 22000;
    }
}
