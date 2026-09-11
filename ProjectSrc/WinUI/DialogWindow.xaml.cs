using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Foundation;

namespace RobloxStudioModManager
{
    // Cancelable event args for the primary button
    public sealed class DialogWindowButtonClickEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }

    // Mirrors System.Windows.Forms.MessageBoxIcon
    public enum DialogIcon
    {
        None,
        Information,
        Warning,
        Error
    }

    // A dedicated top-level window used to present a dialog, replacing
    // WinUI's built-in ContentDialog. Every dialog gets its own real OS window instead.
    public sealed partial class DialogWindow : Window
    {
        // Plays the OS's configured "notification" sound. Lives here rather
        // than a standalone helper file since dialogs are the only thing
        // that ever plays it.
        public static class Sound
        {
            [DllImport("user32.dll")]
            private static extern bool MessageBeep(uint uType);

            private const uint MB_ICONASTERISK = 0x00000040;

            public static void PlayNotification()
            {
                MessageBeep(MB_ICONASTERISK);
            }
        }

        // Raised when the primary button is clicked, before the window
        // closes. Set args.Cancel = true to keep the dialog open (e.g. to
        // show a validation error instead of accepting the input).
        public event TypedEventHandler<DialogWindow, DialogWindowButtonClickEventArgs> PrimaryButtonClick;

        private readonly TaskCompletionSource<ContentDialogResult> tcs = new TaskCompletionSource<ContentDialogResult>();
        private bool resultSet;

        public object DialogContent
        {
            get => ContentHost.Content;
            set => ContentHost.Content = value is string text
                ? new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap }
                : value;
        }

        public string PrimaryButtonText
        {
            get => PrimaryButtonElement.Content as string;
            set
            {
                PrimaryButtonElement.Content = value;
                PrimaryButtonElement.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string CloseButtonText
        {
            get => CloseButtonElement.Content as string;
            set => CloseButtonElement.Content = value;
        }

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.Close;

        // The window this dialog belongs to, when set, the owner is disabled
        // for input while the dialog stays open.
        public Window Owner { get; set; }

        public DialogIcon Icon
        {
            set
            {
                switch (value)
                {
                    case DialogIcon.Information:
                        setIcon("ℹ️");
                        break;
                    case DialogIcon.Warning:
                        setIcon("⚠️");
                        break;
                    case DialogIcon.Error:
                        setIcon("❌");
                        break;
                    default:
                        IconElement.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void setIcon(string emoji)
        {
            IconElement.Text = emoji;
            IconElement.Visibility = Visibility.Visible;
        }

        public DialogWindow()
        {
            InitializeComponent();
            this.ApplyBackdrop();

            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "Icon.ico"));

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsAlwaysOnTop = true;
            }

            Closed += dialogWindow_Closed;
        }

        private void dialogWindow_Closed(object sender, WindowEventArgs args)
        {
            complete(ContentDialogResult.None);

            if (Owner != null)
            {
                WindowPositioning.SetEnabled(Owner, true);
                Owner.Activate();
            }
        }

        private void complete(ContentDialogResult result)
        {
            if (resultSet)
                return;

            resultSet = true;
            tcs.TrySetResult(result);
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new DialogWindowButtonClickEventArgs();
            PrimaryButtonClick?.Invoke(this, args);

            if (args.Cancel)
                return;

            complete(ContentDialogResult.Primary);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            complete(ContentDialogResult.None);
            Close();
        }

        // Shows this dialog as its own window and waits for a button result.
        // Sized to fit its own content.
        // playSound controls whether the OS notification sound plays.
        public Task<ContentDialogResult> ShowAsync(bool playSound = true)
        {
            const double maxContentWidth = 440;
            const double minContentWidth = 300;
            const double minContentHeight = 120;

            RootGrid.Measure(new Size(maxContentWidth, double.PositiveInfinity));
            var desired = RootGrid.DesiredSize;

            int width = (int)Math.Ceiling(Math.Max(minContentWidth, Math.Min(maxContentWidth, desired.Width)));
            int height = (int)Math.Ceiling(Math.Max(minContentHeight, desired.Height));

            if (Owner != null)
            {
                WindowPositioning.SetOwner(this, Owner);
                WindowPositioning.SetEnabled(Owner, false);
            }

            WindowPositioning.ResizeClientAndCenter(AppWindow, width, height, Owner?.AppWindow);

            if (playSound)
                Sound.PlayNotification();

            Activate();

            DispatcherQueue.TryEnqueue(() =>
            {
                var defaultButton = DefaultButton == ContentDialogButton.Primary
                    ? PrimaryButtonElement
                    : CloseButtonElement;

                defaultButton.Focus(FocusState.Programmatic);
            });

            return tcs.Task;
        }

        // Shows a dialog with the given parameter data.
        public static Task<ContentDialogResult> ShowAsync(
            string title,
            object content,
            string primaryButtonText,
            string closeButtonText,
            ContentDialogButton defaultButton = ContentDialogButton.Close,
            DialogIcon icon = DialogIcon.None,
            Window owner = null,
            bool playSound = true)
        {
            var dialog = new DialogWindow
            {
                Title = title,
                DialogContent = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = defaultButton,
                Icon = icon,
                Owner = owner
            };

            return dialog.ShowAsync(playSound);
        }
    }
}
