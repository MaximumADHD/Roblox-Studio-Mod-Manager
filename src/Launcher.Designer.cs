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
            this.programLogo = new System.Windows.Forms.PictureBox();
            this.manageMods = new System.Windows.Forms.Button();
            this.branchSelect = new System.Windows.Forms.ComboBox();
            this.branchToUse = new System.Windows.Forms.Label();
            this.forceRebuild = new System.Windows.Forms.CheckBox();
            this.openStudioDirectory = new System.Windows.Forms.CheckBox();
            this.openFlagEditor = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.programLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // launchStudio
            // 
            this.launchStudio.AccessibleName = "Launch Roblox Studio";
            this.launchStudio.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.launchStudio.Cursor = System.Windows.Forms.Cursors.Default;
            this.launchStudio.Location = new System.Drawing.Point(25, 119);
            this.launchStudio.Name = "launchStudio";
            this.launchStudio.Size = new System.Drawing.Size(140, 23);
            this.launchStudio.TabIndex = 6;
            this.launchStudio.Text = "Launch Roblox Studio";
            this.launchStudio.UseVisualStyleBackColor = true;
            this.launchStudio.Click += new System.EventHandler(this.launchStudio_Click);
            this.launchStudio.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // programLogo
            // 
            this.programLogo.AccessibleName = "Roblox Studio Logo";
            this.programLogo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.programLogo.BackColor = System.Drawing.Color.Transparent;
            this.programLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.programLogo.Image = global::RobloxStudioModManager.Properties.Resources.Logo;
            this.programLogo.InitialImage = null;
            this.programLogo.Location = new System.Drawing.Point(25, 12);
            this.programLogo.Name = "programLogo";
            this.programLogo.Size = new System.Drawing.Size(140, 101);
            this.programLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.programLogo.TabIndex = 8;
            this.programLogo.TabStop = false;
            // 
            // manageMods
            // 
            this.manageMods.AccessibleName = "Manage Mod Files";
            this.manageMods.Cursor = System.Windows.Forms.Cursors.Default;
            this.manageMods.Location = new System.Drawing.Point(25, 148);
            this.manageMods.Name = "manageMods";
            this.manageMods.Size = new System.Drawing.Size(140, 23);
            this.manageMods.TabIndex = 9;
            this.manageMods.Text = "Manage Mod Files";
            this.manageMods.UseVisualStyleBackColor = true;
            this.manageMods.Click += new System.EventHandler(this.manageMods_Click);
            this.manageMods.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // branchSelect
            // 
            this.branchSelect.AccessibleName = "Branch Selector";
            this.branchSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.branchSelect.FormattingEnabled = true;
            this.branchSelect.Items.AddRange(new object[] {
            "roblox",
            "gametest1.robloxlabs",
            "gametest2.robloxlabs",
            "gametest3.robloxlabs",
            "gametest4.robloxlabs",
            "gametest5.robloxlabs"});
            this.branchSelect.Location = new System.Drawing.Point(25, 192);
            this.branchSelect.Name = "branchSelect";
            this.branchSelect.Size = new System.Drawing.Size(140, 21);
            this.branchSelect.TabIndex = 10;
            this.branchSelect.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // branchToUse
            // 
            this.branchToUse.AutoSize = true;
            this.branchToUse.BackColor = System.Drawing.Color.Transparent;
            this.branchToUse.CausesValidation = false;
            this.branchToUse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.branchToUse.ForeColor = System.Drawing.SystemColors.ControlText;
            this.branchToUse.Location = new System.Drawing.Point(22, 174);
            this.branchToUse.Name = "branchToUse";
            this.branchToUse.Size = new System.Drawing.Size(79, 13);
            this.branchToUse.TabIndex = 11;
            this.branchToUse.Text = "Branch to use: ";
            // 
            // forceRebuild
            // 
            this.forceRebuild.AccessibleName = "Force Client Rebuild";
            this.forceRebuild.AutoSize = true;
            this.forceRebuild.Location = new System.Drawing.Point(25, 218);
            this.forceRebuild.Margin = new System.Windows.Forms.Padding(2);
            this.forceRebuild.Name = "forceRebuild";
            this.forceRebuild.Size = new System.Drawing.Size(121, 17);
            this.forceRebuild.TabIndex = 12;
            this.forceRebuild.Text = "Force Client Rebuild";
            this.forceRebuild.UseVisualStyleBackColor = true;
            this.forceRebuild.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // openStudioDirectory
            // 
            this.openStudioDirectory.AccessibleName = "Just Open Studio Path";
            this.openStudioDirectory.AutoSize = true;
            this.openStudioDirectory.Location = new System.Drawing.Point(25, 239);
            this.openStudioDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.openStudioDirectory.Name = "openStudioDirectory";
            this.openStudioDirectory.Size = new System.Drawing.Size(132, 17);
            this.openStudioDirectory.TabIndex = 14;
            this.openStudioDirectory.Text = "Just Open Studio Path";
            this.openStudioDirectory.UseVisualStyleBackColor = true;
            this.openStudioDirectory.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // openFlagEditor
            // 
            this.openFlagEditor.AccessibleName = "Open Flag Editor";
            this.openFlagEditor.Location = new System.Drawing.Point(25, 257);
            this.openFlagEditor.Name = "openFlagEditor";
            this.openFlagEditor.Size = new System.Drawing.Size(140, 23);
            this.openFlagEditor.TabIndex = 15;
            this.openFlagEditor.Text = "Open Flag Editor";
            this.openFlagEditor.UseVisualStyleBackColor = true;
            this.openFlagEditor.Click += new System.EventHandler(this.editFVariables_Click);
            this.openFlagEditor.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(189, 288);
            this.Controls.Add(this.openFlagEditor);
            this.Controls.Add(this.openStudioDirectory);
            this.Controls.Add(this.forceRebuild);
            this.Controls.Add(this.branchToUse);
            this.Controls.Add(this.branchSelect);
            this.Controls.Add(this.manageMods);
            this.Controls.Add(this.programLogo);
            this.Controls.Add(this.launchStudio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = global::RobloxStudioModManager.Properties.Resources.Icon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Launcher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.programLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button launchStudio;
        private System.Windows.Forms.PictureBox programLogo;
        private System.Windows.Forms.Button manageMods;
        private System.Windows.Forms.ComboBox branchSelect;
        private System.Windows.Forms.Label branchToUse;
        private System.Windows.Forms.CheckBox forceRebuild;
        private System.Windows.Forms.CheckBox openStudioDirectory;
        private System.Windows.Forms.Button openFlagEditor;
    }
}

