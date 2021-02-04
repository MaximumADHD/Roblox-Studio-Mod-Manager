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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.launchStudio = new System.Windows.Forms.Button();
            this.manageMods = new System.Windows.Forms.Button();
            this.branchSelect = new System.Windows.Forms.ComboBox();
            this.branchLabel = new System.Windows.Forms.Label();
            this.forceRebuild = new System.Windows.Forms.CheckBox();
            this.openFlagEditor = new System.Windows.Forms.Button();
            this.editExplorerIcons = new System.Windows.Forms.Button();
            this.logo = new System.Windows.Forms.PictureBox();
            this.openStudioDirectory = new System.Windows.Forms.CheckBox();
            this.targetVersionLabel = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.targetVersion = new System.Windows.Forms.ComboBox();
            this.logoPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.logoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // launchStudio
            // 
            this.launchStudio.AccessibleName = "Launch Roblox Studio";
            this.launchStudio.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.launchStudio.Cursor = System.Windows.Forms.Cursors.Default;
            this.launchStudio.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.launchStudio.Location = new System.Drawing.Point(19, 192);
            this.launchStudio.Margin = new System.Windows.Forms.Padding(10, 6, 5, 6);
            this.launchStudio.Name = "launchStudio";
            this.launchStudio.Size = new System.Drawing.Size(237, 44);
            this.launchStudio.TabIndex = 6;
            this.launchStudio.Text = "Launch Studio";
            this.launchStudio.UseVisualStyleBackColor = true;
            this.launchStudio.Click += new System.EventHandler(this.launchStudio_Click);
            // 
            // manageMods
            // 
            this.manageMods.AccessibleName = "Open Mod Folder";
            this.manageMods.Cursor = System.Windows.Forms.Cursors.Default;
            this.manageMods.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.manageMods.Location = new System.Drawing.Point(19, 248);
            this.manageMods.Margin = new System.Windows.Forms.Padding(10, 6, 5, 6);
            this.manageMods.Name = "manageMods";
            this.manageMods.Size = new System.Drawing.Size(237, 44);
            this.manageMods.TabIndex = 9;
            this.manageMods.Text = "Open Mod Folder";
            this.manageMods.UseVisualStyleBackColor = true;
            this.manageMods.Click += new System.EventHandler(this.manageMods_Click);
            // 
            // branchSelect
            // 
            this.branchSelect.AccessibleName = "Branch Selector";
            this.branchSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.branchSelect.FormattingEnabled = true;
            this.branchSelect.Items.AddRange(new object[] {
            "roblox",
            "sitetest1.robloxlabs",
            "sitetest2.robloxlabs",
            "sitetest3.robloxlabs"});
            this.branchSelect.Location = new System.Drawing.Point(274, 218);
            this.branchSelect.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.branchSelect.Name = "branchSelect";
            this.branchSelect.Size = new System.Drawing.Size(251, 33);
            this.branchSelect.TabIndex = 10;
            this.branchSelect.SelectedIndexChanged += new System.EventHandler(this.branchSelect_SelectedIndexChanged);
            // 
            // branchLabel
            // 
            this.branchLabel.AutoSize = true;
            this.branchLabel.BackColor = System.Drawing.Color.Transparent;
            this.branchLabel.CausesValidation = false;
            this.branchLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.branchLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.branchLabel.Location = new System.Drawing.Point(269, 192);
            this.branchLabel.Margin = new System.Windows.Forms.Padding(0);
            this.branchLabel.Name = "branchLabel";
            this.branchLabel.Size = new System.Drawing.Size(130, 25);
            this.branchLabel.TabIndex = 11;
            this.branchLabel.Text = "Studio Branch: ";
            this.branchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // forceRebuild
            // 
            this.forceRebuild.AccessibleName = "Force Client Rebuild";
            this.forceRebuild.AutoSize = true;
            this.forceRebuild.Location = new System.Drawing.Point(274, 338);
            this.forceRebuild.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.forceRebuild.Name = "forceRebuild";
            this.forceRebuild.Size = new System.Drawing.Size(190, 29);
            this.forceRebuild.TabIndex = 12;
            this.forceRebuild.Text = "Force Reinstallation";
            this.forceRebuild.UseVisualStyleBackColor = true;
            // 
            // openFlagEditor
            // 
            this.openFlagEditor.AccessibleName = "Open Flag Editor";
            this.openFlagEditor.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.openFlagEditor.Location = new System.Drawing.Point(19, 304);
            this.openFlagEditor.Margin = new System.Windows.Forms.Padding(10, 6, 5, 6);
            this.openFlagEditor.Name = "openFlagEditor";
            this.openFlagEditor.Size = new System.Drawing.Size(237, 44);
            this.openFlagEditor.TabIndex = 15;
            this.openFlagEditor.Text = "Edit Fast Flags";
            this.openFlagEditor.UseVisualStyleBackColor = true;
            this.openFlagEditor.Click += new System.EventHandler(this.editFVariables_Click);
            // 
            // editExplorerIcons
            // 
            this.editExplorerIcons.AccessibleName = "Open Flag Editor";
            this.editExplorerIcons.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.editExplorerIcons.Location = new System.Drawing.Point(19, 360);
            this.editExplorerIcons.Margin = new System.Windows.Forms.Padding(10, 6, 5, 6);
            this.editExplorerIcons.Name = "editExplorerIcons";
            this.editExplorerIcons.Size = new System.Drawing.Size(237, 44);
            this.editExplorerIcons.TabIndex = 16;
            this.editExplorerIcons.Text = "Edit Class Icons";
            this.editExplorerIcons.UseVisualStyleBackColor = true;
            this.editExplorerIcons.Click += new System.EventHandler(this.editExplorerIcons_Click);
            // 
            // logo
            // 
            this.logo.Dock = System.Windows.Forms.DockStyle.Left;
            this.logo.Image = RobloxStudioModManager.Properties.Resources.Logo;
            this.logo.Location = new System.Drawing.Point(5, 6);
            this.logo.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(150, 150);
            this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.logo.TabIndex = 19;
            this.logo.TabStop = false;
            // 
            // openStudioDirectory
            // 
            this.openStudioDirectory.AccessibleName = "Just Open Studio Path";
            this.openStudioDirectory.AutoSize = true;
            this.openStudioDirectory.Location = new System.Drawing.Point(274, 375);
            this.openStudioDirectory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.openStudioDirectory.Name = "openStudioDirectory";
            this.openStudioDirectory.Size = new System.Drawing.Size(250, 29);
            this.openStudioDirectory.TabIndex = 14;
            this.openStudioDirectory.Text = "Just Open Studio Directory";
            this.openStudioDirectory.UseVisualStyleBackColor = true;
            // 
            // targetVersionLabel
            // 
            this.targetVersionLabel.AutoSize = true;
            this.targetVersionLabel.BackColor = System.Drawing.Color.Transparent;
            this.targetVersionLabel.CausesValidation = false;
            this.targetVersionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.targetVersionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.targetVersionLabel.Location = new System.Drawing.Point(269, 257);
            this.targetVersionLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.targetVersionLabel.Name = "targetVersionLabel";
            this.targetVersionLabel.Size = new System.Drawing.Size(127, 25);
            this.targetVersionLabel.TabIndex = 17;
            this.targetVersionLabel.Text = "Target Version:";
            // 
            // title
            // 
            this.title.Font = new System.Drawing.Font("Segoe UI Light", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.title.Location = new System.Drawing.Point(165, 0);
            this.title.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(263, 162);
            this.title.TabIndex = 20;
            this.title.Text = "Roblox Studio\r\nMod Manager";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // targetVersion
            // 
            this.targetVersion.AccessibleName = "Target Version";
            this.targetVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetVersion.FormattingEnabled = true;
            this.targetVersion.Items.AddRange(new object[] {
            "(Use Latest)"});
            this.targetVersion.Location = new System.Drawing.Point(274, 283);
            this.targetVersion.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.targetVersion.Name = "targetVersion";
            this.targetVersion.Size = new System.Drawing.Size(251, 33);
            this.targetVersion.TabIndex = 18;
            this.targetVersion.SelectedIndexChanged += new System.EventHandler(this.targetVersion_SelectedIndexChanged);
            // 
            // logoPanel
            // 
            this.logoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logoPanel.AutoSize = true;
            this.logoPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.logoPanel.Controls.Add(this.logo);
            this.logoPanel.Controls.Add(this.title);
            this.logoPanel.Location = new System.Drawing.Point(59, 12);
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(433, 162);
            this.logoPanel.TabIndex = 21;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(541, 429);
            this.Controls.Add(this.logoPanel);
            this.Controls.Add(this.targetVersion);
            this.Controls.Add(this.targetVersionLabel);
            this.Controls.Add(this.editExplorerIcons);
            this.Controls.Add(this.openFlagEditor);
            this.Controls.Add(this.openStudioDirectory);
            this.Controls.Add(this.forceRebuild);
            this.Controls.Add(this.branchLabel);
            this.Controls.Add(this.branchSelect);
            this.Controls.Add(this.manageMods);
            this.Controls.Add(this.launchStudio);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.MaximizeBox = false;
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox Studio Mod Manager";
            this.Load += new System.EventHandler(this.Launcher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.logoPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button launchStudio;
        private System.Windows.Forms.Button manageMods;
        private System.Windows.Forms.ComboBox branchSelect;
        private System.Windows.Forms.Label branchLabel;
        private System.Windows.Forms.CheckBox forceRebuild;
        private System.Windows.Forms.Button openFlagEditor;
        private System.Windows.Forms.Button editExplorerIcons;
        private System.Windows.Forms.PictureBox logo;
        private System.Windows.Forms.CheckBox openStudioDirectory;
        private System.Windows.Forms.Label targetVersionLabel;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.ComboBox targetVersion;
        private System.Windows.Forms.FlowLayoutPanel logoPanel;
    }
}

