using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Controls;

// These classes exist so we can have dialogs that play the default system dialog sound,
// when they open. WinUI doesn't provide equal functionality by default, unfortunately.

namespace RobloxStudioModManager.WinUI
{
    internal static class SystemSound
    {
        [DllImport("user32.dll")]
        private static extern bool MessageBeep(uint uType);

        private const uint MB_ICONASTERISK = 0x00000040;

        // Plays the OS's configured "notification" sound.
        public static void PlayNotification()
        {
            MessageBeep(MB_ICONASTERISK);
        }
    }

    internal static class ContentDialogExtensions
    {
        // A simple wrapper to play the notification sound alongside showing the dialog.
        public static async Task<ContentDialogResult> ShowWithSoundAsync(this ContentDialog dialog)
        {
            SystemSound.PlayNotification();
            return await dialog.ShowAsync();
        }
    }
}
