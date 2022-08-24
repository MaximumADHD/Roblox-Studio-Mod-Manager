namespace RobloxStudioModManager
{
    partial class Launcher
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.launchStudio = new System.Windows.Forms.Button();
            this.manageMods = new System.Windows.Forms.Button();
            this.channelSelect = new System.Windows.Forms.ComboBox();
            this.channelLabel = new System.Windows.Forms.Label();
            this.forceRebuild = new System.Windows.Forms.CheckBox();
            this.openFlagEditor = new System.Windows.Forms.Button();
            this.editExplorerIcons = new System.Windows.Forms.Button();
            this.openStudioDirectory = new System.Windows.Forms.CheckBox();
            this.targetVersionLabel = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.targetVersion = new System.Windows.Forms.ComboBox();
            this.logo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.SuspendLayout();
            // 
            // launchStudio
            // 
            this.launchStudio.AccessibleName = "Launch Roblox Studio";
            this.launchStudio.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.launchStudio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.launchStudio.Cursor = System.Windows.Forms.Cursors.Default;
            this.launchStudio.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.launchStudio.Location = new System.Drawing.Point(11, 127);
            this.launchStudio.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.launchStudio.Name = "launchStudio";
            this.launchStudio.Size = new System.Drawing.Size(142, 23);
            this.launchStudio.TabIndex = 6;
            this.launchStudio.Text = "Launch Studio";
            this.launchStudio.UseVisualStyleBackColor = true;
            this.launchStudio.Click += new System.EventHandler(this.launchStudio_Click);
            // 
            // manageMods
            // 
            this.manageMods.AccessibleName = "Open Mod Folder";
            this.manageMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.manageMods.Cursor = System.Windows.Forms.Cursors.Default;
            this.manageMods.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manageMods.Location = new System.Drawing.Point(11, 155);
            this.manageMods.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.manageMods.Name = "manageMods";
            this.manageMods.Size = new System.Drawing.Size(142, 23);
            this.manageMods.TabIndex = 9;
            this.manageMods.Text = "Open Mod Folder";
            this.manageMods.UseVisualStyleBackColor = true;
            this.manageMods.Click += new System.EventHandler(this.manageMods_Click);
            // 
            // channelSelect
            // 
            this.channelSelect.AccessibleName = "Channel Selector";
            this.channelSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.channelSelect.FormattingEnabled = true;
            this.channelSelect.Items.AddRange(new object[] {
            "zLive",
            "zCanary",
            "zIntegration",
            "zQtitanStudioRelease"});
            this.channelSelect.Location = new System.Drawing.Point(175, 138);
            this.channelSelect.Name = "channelSelect";
            this.channelSelect.Size = new System.Drawing.Size(152, 21);
            this.channelSelect.TabIndex = 10;
            this.channelSelect.Text = "";
            this.channelSelect.SelectedIndexChanged += new System.EventHandler(this.channelSelect_SelectedIndexChanged);
            this.channelSelect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.channelSelect_KeyDown);
            // 
            // channelLabel
            // 
            this.channelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.channelLabel.AutoSize = true;
            this.channelLabel.BackColor = System.Drawing.Color.Transparent;
            this.channelLabel.CausesValidation = false;
            this.channelLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.channelLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.channelLabel.Location = new System.Drawing.Point(171, 120);
            this.channelLabel.Margin = new System.Windows.Forms.Padding(0);
            this.channelLabel.Name = "channelLabel";
            this.channelLabel.Size = new System.Drawing.Size(57, 15);
            this.channelLabel.TabIndex = 11;
            this.channelLabel.Text = "Channel: ";
            this.channelLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // forceRebuild
            // 
            this.forceRebuild.AccessibleName = "Force Client Rebuild";
            this.forceRebuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.forceRebuild.AutoSize = true;
            this.forceRebuild.Location = new System.Drawing.Point(174, 204);
            this.forceRebuild.Margin = new System.Windows.Forms.Padding(2);
            this.forceRebuild.Name = "forceRebuild";
            this.forceRebuild.Size = new System.Drawing.Size(119, 17);
            this.forceRebuild.TabIndex = 12;
            this.forceRebuild.Text = "Force Reinstallation";
            this.forceRebuild.UseVisualStyleBackColor = true;
            // 
            // openFlagEditor
            // 
            this.openFlagEditor.AccessibleName = "Open Flag Editor";
            this.openFlagEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.openFlagEditor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openFlagEditor.Location = new System.Drawing.Point(11, 185);
            this.openFlagEditor.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.openFlagEditor.Name = "openFlagEditor";
            this.openFlagEditor.Size = new System.Drawing.Size(142, 23);
            this.openFlagEditor.TabIndex = 15;
            this.openFlagEditor.Text = "Edit Fast Flags";
            this.openFlagEditor.UseVisualStyleBackColor = true;
            this.openFlagEditor.Click += new System.EventHandler(this.editFVariables_Click);
            // 
            // editExplorerIcons
            // 
            this.editExplorerIcons.AccessibleName = "Open Flag Editor";
            this.editExplorerIcons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editExplorerIcons.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.editExplorerIcons.Location = new System.Drawing.Point(11, 214);
            this.editExplorerIcons.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.editExplorerIcons.Name = "editExplorerIcons";
            this.editExplorerIcons.Size = new System.Drawing.Size(142, 23);
            this.editExplorerIcons.TabIndex = 16;
            this.editExplorerIcons.Text = "Edit Class Icons";
            this.editExplorerIcons.UseVisualStyleBackColor = true;
            this.editExplorerIcons.Click += new System.EventHandler(this.editExplorerIcons_Click);
            // 
            // openStudioDirectory
            // 
            this.openStudioDirectory.AccessibleName = "Just Open Studio Path";
            this.openStudioDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.openStudioDirectory.AutoSize = true;
            this.openStudioDirectory.Location = new System.Drawing.Point(174, 224);
            this.openStudioDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.openStudioDirectory.Name = "openStudioDirectory";
            this.openStudioDirectory.Size = new System.Drawing.Size(152, 17);
            this.openStudioDirectory.TabIndex = 14;
            this.openStudioDirectory.Text = "Just Open Studio Directory";
            this.openStudioDirectory.UseVisualStyleBackColor = true;
            // 
            // targetVersionLabel
            // 
            this.targetVersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.targetVersionLabel.AutoSize = true;
            this.targetVersionLabel.BackColor = System.Drawing.Color.Transparent;
            this.targetVersionLabel.CausesValidation = false;
            this.targetVersionLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.targetVersionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.targetVersionLabel.Location = new System.Drawing.Point(171, 162);
            this.targetVersionLabel.Name = "targetVersionLabel";
            this.targetVersionLabel.Size = new System.Drawing.Size(83, 15);
            this.targetVersionLabel.TabIndex = 17;
            this.targetVersionLabel.Text = "Target Version:";
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Segoe UI Light", 20F);
            this.title.Location = new System.Drawing.Point(136, 27);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(176, 74);
            this.title.TabIndex = 20;
            this.title.Text = "Roblox Studio\r\nMod Manager";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // targetVersion
            // 
            this.targetVersion.AccessibleName = "Target Version";
            this.targetVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.targetVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetVersion.FormattingEnabled = true;
            this.targetVersion.Items.AddRange(new object[] {
            "(Use Latest)"});
            this.targetVersion.Location = new System.Drawing.Point(174, 179);
            this.targetVersion.Name = "targetVersion";
            this.targetVersion.Size = new System.Drawing.Size(152, 21);
            this.targetVersion.TabIndex = 18;
            this.targetVersion.SelectedIndexChanged += new System.EventHandler(this.targetVersion_SelectedIndexChanged);
            // 
            // logo
            // 
            this.logo.BackgroundImage = global::RobloxStudioModManager.Properties.Resources.Logo;
            this.logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.logo.Location = new System.Drawing.Point(37, 18);
            this.logo.Margin = new System.Windows.Forms.Padding(2);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(90, 88);
            this.logo.TabIndex = 22;
            this.logo.TabStop = false;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(335, 250);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.title);
            this.Controls.Add(this.targetVersion);
            this.Controls.Add(this.targetVersionLabel);
            this.Controls.Add(this.editExplorerIcons);
            this.Controls.Add(this.openFlagEditor);
            this.Controls.Add(this.openStudioDirectory);
            this.Controls.Add(this.forceRebuild);
            this.Controls.Add(this.channelLabel);
            this.Controls.Add(this.channelSelect);
            this.Controls.Add(this.manageMods);
            this.Controls.Add(this.launchStudio);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RobloxStudioModManager.Properties.Resources.Icon;
            this.MaximizeBox = false;
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox Studio Mod Manager";
            this.Load += new System.EventHandler(this.Launcher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button launchStudio;
        private System.Windows.Forms.Button manageMods;
        private System.Windows.Forms.ComboBox channelSelect;
        private System.Windows.Forms.Label channelLabel;
        private System.Windows.Forms.CheckBox forceRebuild;
        private System.Windows.Forms.Button openFlagEditor;
        private System.Windows.Forms.Button editExplorerIcons;
        private System.Windows.Forms.CheckBox openStudioDirectory;
        private System.Windows.Forms.Label targetVersionLabel;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.ComboBox targetVersion;
        private System.Windows.Forms.PictureBox logo;
    }
}

