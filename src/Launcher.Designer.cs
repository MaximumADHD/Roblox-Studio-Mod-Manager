namespace RobloxModManager
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.manageMods = new System.Windows.Forms.Button();
            this.dataBaseSelect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.forceRebuild = new System.Windows.Forms.CheckBox();
            this.disableVR = new System.Windows.Forms.CheckBox();
            this.openStudioDirectory = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // launchStudio
            // 
            this.launchStudio.AccessibleName = "Launch ROBLOX Studio";
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
            // pictureBox1
            // 
            this.pictureBox1.AccessibleName = "ROBLOX Studio Logo";
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(25, -7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(140, 128);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
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
            // dataBaseSelect
            // 
            this.dataBaseSelect.AccessibleName = "Database Selector";
            this.dataBaseSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataBaseSelect.FormattingEnabled = true;
            this.dataBaseSelect.Items.AddRange(new object[] {
            "roblox",
            "gametest1.robloxlabs",
            "gametest2.robloxlabs",
            "gametest3.robloxlabs",
            "gametest4.robloxlabs",
            "gametest5.robloxlabs"});
            this.dataBaseSelect.Location = new System.Drawing.Point(25, 192);
            this.dataBaseSelect.Name = "dataBaseSelect";
            this.dataBaseSelect.Size = new System.Drawing.Size(140, 21);
            this.dataBaseSelect.TabIndex = 10;
            this.dataBaseSelect.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.dataBaseSelect.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.CausesValidation = false;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(22, 174);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Database to use: ";
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
            // disableVR
            // 
            this.disableVR.AccessibleName = "Disable VR";
            this.disableVR.AutoSize = true;
            this.disableVR.Checked = true;
            this.disableVR.CheckState = System.Windows.Forms.CheckState.Checked;
            this.disableVR.Location = new System.Drawing.Point(25, 260);
            this.disableVR.Margin = new System.Windows.Forms.Padding(2);
            this.disableVR.Name = "disableVR";
            this.disableVR.Size = new System.Drawing.Size(79, 17);
            this.disableVR.TabIndex = 13;
            this.disableVR.Text = "Disable VR";
            this.disableVR.UseVisualStyleBackColor = true;
            this.disableVR.Visible = false;
            this.disableVR.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // openStudioDirectory
            // 
            this.openStudioDirectory.AccessibleName = "Open Studio Directory";
            this.openStudioDirectory.AutoSize = true;
            this.openStudioDirectory.Location = new System.Drawing.Point(25, 239);
            this.openStudioDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.openStudioDirectory.Name = "openStudioDirectory";
            this.openStudioDirectory.Size = new System.Drawing.Size(130, 17);
            this.openStudioDirectory.TabIndex = 14;
            this.openStudioDirectory.Text = "Open Studio Directory";
            this.openStudioDirectory.UseVisualStyleBackColor = true;
            this.openStudioDirectory.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.onHelpRequested);
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(189, 265);
            this.Controls.Add(this.openStudioDirectory);
            this.Controls.Add(this.disableVR);
            this.Controls.Add(this.forceRebuild);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataBaseSelect);
            this.Controls.Add(this.manageMods);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.launchStudio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox Studio Mod Manager";
            this.Load += new System.EventHandler(this.Launcher_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button launchStudio;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button manageMods;
        private System.Windows.Forms.ComboBox dataBaseSelect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox forceRebuild;
        private System.Windows.Forms.CheckBox disableVR;
        private System.Windows.Forms.CheckBox openStudioDirectory;
    }
}

