using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RobloxStudioModManager
{
    public partial class FlagCreator : Form
    {
        public CustomFlag Result { get; private set; }
        private static readonly string[] classes = new string[3] { "F", "DF", "SF" };

        public FlagCreator()
        {
            InitializeComponent();
            flagType.SelectedIndex = 0;
            flagClass.SelectedIndex = 0;
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            string classType = classes[flagClass.SelectedIndex];
            string type = classType + flagType.SelectedItem.ToString();
            string name = Regex.Replace(flagName.Text, "[^A-z0-9_]", "");

            if (flagName.Text != name)
                flagName.Text = name;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show
                (
                    $"Please enter a name for the {type}!",
                    "Invalid submission!",

                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            Result = new CustomFlag(type, name);
            DialogResult = DialogResult.OK;

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
