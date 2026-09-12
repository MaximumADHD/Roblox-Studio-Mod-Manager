using System.Text.RegularExpressions;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RobloxStudioModManager
{
    public sealed partial class FlagCreatorDialog : UserControl
    {
        private static readonly string[] classes = { "F", "DF", "SF" };

        public CustomFlag Result { get; private set; }

        public FlagCreatorDialog()
        {
            InitializeComponent();
            FlagClassBox.SelectedIndex = 0;
            FlagTypeBox.SelectedIndex = 0;
        }

        // Validates the form and sets Result on success. Returns false (and
        // shows an inline error) when the input isn't ready, the hosting
        // DialogWindow's primary button uses this to cancel its own close.
        public bool Validate()
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
                return false;
            }

            Result = new CustomFlag(type, name);
            return true;
        }
    }
}
