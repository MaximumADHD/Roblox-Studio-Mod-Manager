using System.Text.RegularExpressions;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using RobloxStudioModManager;

namespace RobloxStudioModManager.WinUI
{
    public sealed partial class FlagCreatorDialog : ContentDialog
    {
        private static readonly string[] classes = { "F", "DF", "SF" };

        public CustomFlag Result { get; private set; }

        public FlagCreatorDialog()
        {
            InitializeComponent();
            FlagClassBox.SelectedIndex = 0;
            FlagTypeBox.SelectedIndex = 0;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string classType = classes[FlagClassBox.SelectedIndex];
            string typeItem = ((ComboBoxItem)FlagTypeBox.SelectedItem).Content.ToString();
            string type = classType + typeItem;
            string name = Regex.Replace(FlagNameBox.Text, "[^A-z0-9_]", "");

            if (FlagNameBox.Text != name)
                FlagNameBox.Text = name;

            if (string.IsNullOrEmpty(name))
            {
                ErrorText.Text = $"Please enter a name for the {type}!";
                ErrorText.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            Result = new CustomFlag(type, name);
        }
    }
}
