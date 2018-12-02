namespace RobloxStudioModManager
{
    partial class ExplorerIconEditor
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
            this.iconContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.selectIcon = new System.Windows.Forms.Label();
            this.enableIconOverride = new System.Windows.Forms.CheckBox();
            this.editIcon = new System.Windows.Forms.Button();
            this.selectedIcon = new System.Windows.Forms.PictureBox();
            this.restoreOriginal = new System.Windows.Forms.Button();
            this.themeSwitcher = new System.Windows.Forms.Button();
            this.memoryStatus = new System.Windows.Forms.Label();
            this.studioStatus = new System.Windows.Forms.Label();
            this.errors = new System.Windows.Forms.Label();
            this.showModified = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.selectedIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // iconContainer
            // 
            this.iconContainer.AutoScroll = true;
            this.iconContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.iconContainer.Location = new System.Drawing.Point(12, 28);
            this.iconContainer.MaximumSize = new System.Drawing.Size(272, 9999);
            this.iconContainer.Name = "iconContainer";
            this.iconContainer.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.iconContainer.Size = new System.Drawing.Size(259, 170);
            this.iconContainer.TabIndex = 0;
            // 
            // selectIcon
            // 
            this.selectIcon.AutoSize = true;
            this.selectIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectIcon.Location = new System.Drawing.Point(8, 4);
            this.selectIcon.Name = "selectIcon";
            this.selectIcon.Size = new System.Drawing.Size(100, 20);
            this.selectIcon.TabIndex = 2;
            this.selectIcon.Text = "Select Icon";
            // 
            // enableIconOverride
            // 
            this.enableIconOverride.AutoSize = true;
            this.enableIconOverride.Location = new System.Drawing.Point(64, 208);
            this.enableIconOverride.Name = "enableIconOverride";
            this.enableIconOverride.Size = new System.Drawing.Size(90, 17);
            this.enableIconOverride.TabIndex = 4;
            this.enableIconOverride.Text = "Override Icon";
            this.enableIconOverride.UseVisualStyleBackColor = true;
            this.enableIconOverride.CheckedChanged += new System.EventHandler(this.enableIconOverride_CheckedChanged);
            // 
            // editIcon
            // 
            this.editIcon.Location = new System.Drawing.Point(63, 229);
            this.editIcon.Name = "editIcon";
            this.editIcon.Size = new System.Drawing.Size(90, 23);
            this.editIcon.TabIndex = 5;
            this.editIcon.Text = "Edit Icon 0";
            this.editIcon.UseVisualStyleBackColor = true;
            this.editIcon.Click += new System.EventHandler(this.editIcon_Click);
            // 
            // selectedIcon
            // 
            this.selectedIcon.BackColor = System.Drawing.Color.White;
            this.selectedIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.selectedIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.selectedIcon.Location = new System.Drawing.Point(12, 206);
            this.selectedIcon.Name = "selectedIcon";
            this.selectedIcon.Size = new System.Drawing.Size(46, 46);
            this.selectedIcon.TabIndex = 3;
            this.selectedIcon.TabStop = false;
            // 
            // restoreOriginal
            // 
            this.restoreOriginal.Enabled = false;
            this.restoreOriginal.Location = new System.Drawing.Point(159, 229);
            this.restoreOriginal.Name = "restoreOriginal";
            this.restoreOriginal.Size = new System.Drawing.Size(112, 23);
            this.restoreOriginal.TabIndex = 6;
            this.restoreOriginal.Text = "Restore Original";
            this.restoreOriginal.UseCompatibleTextRendering = true;
            this.restoreOriginal.UseVisualStyleBackColor = true;
            this.restoreOriginal.Click += new System.EventHandler(this.restoreOriginal_Click);
            // 
            // themeSwitcher
            // 
            this.themeSwitcher.Location = new System.Drawing.Point(159, 204);
            this.themeSwitcher.Name = "themeSwitcher";
            this.themeSwitcher.Size = new System.Drawing.Size(112, 23);
            this.themeSwitcher.TabIndex = 7;
            this.themeSwitcher.Text = "Theme: Light";
            this.themeSwitcher.UseCompatibleTextRendering = true;
            this.themeSwitcher.UseVisualStyleBackColor = true;
            this.themeSwitcher.Click += new System.EventHandler(this.themeSwitcher_Click);
            // 
            // memoryStatus
            // 
            this.memoryStatus.AutoSize = true;
            this.memoryStatus.Location = new System.Drawing.Point(9, 259);
            this.memoryStatus.Name = "memoryStatus";
            this.memoryStatus.Size = new System.Drawing.Size(134, 13);
            this.memoryStatus.TabIndex = 8;
            this.memoryStatus.Text = "Memory Budget: Loading...";
            // 
            // studioStatus
            // 
            this.studioStatus.AutoSize = true;
            this.studioStatus.Location = new System.Drawing.Point(9, 272);
            this.studioStatus.Name = "studioStatus";
            this.studioStatus.Size = new System.Drawing.Size(123, 13);
            this.studioStatus.TabIndex = 9;
            this.studioStatus.Text = "Studio Status: Loading...";
            // 
            // errors
            // 
            this.errors.AutoSize = true;
            this.errors.ForeColor = System.Drawing.Color.Red;
            this.errors.Location = new System.Drawing.Point(9, 290);
            this.errors.Name = "errors";
            this.errors.Size = new System.Drawing.Size(0, 13);
            this.errors.TabIndex = 10;
            // 
            // showModified
            // 
            this.showModified.AutoSize = true;
            this.showModified.Location = new System.Drawing.Point(146, 7);
            this.showModified.Name = "showModified";
            this.showModified.Size = new System.Drawing.Size(125, 17);
            this.showModified.TabIndex = 11;
            this.showModified.Text = "Show Modified Icons";
            this.showModified.UseVisualStyleBackColor = true;
            this.showModified.CheckedChanged += new System.EventHandler(this.showModified_CheckedChanged);
            // 
            // ExplorerIconEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 328);
            this.Controls.Add(this.showModified);
            this.Controls.Add(this.errors);
            this.Controls.Add(this.studioStatus);
            this.Controls.Add(this.memoryStatus);
            this.Controls.Add(this.themeSwitcher);
            this.Controls.Add(this.restoreOriginal);
            this.Controls.Add(this.editIcon);
            this.Controls.Add(this.enableIconOverride);
            this.Controls.Add(this.selectedIcon);
            this.Controls.Add(this.selectIcon);
            this.Controls.Add(this.iconContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::RobloxStudioModManager.Properties.Resources.Icon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExplorerIconEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Explorer Icon Editor";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ExplorerIconEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.selectedIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel iconContainer;
        private System.Windows.Forms.Label selectIcon;
        private System.Windows.Forms.CheckBox enableIconOverride;
        private System.Windows.Forms.Button editIcon;
        private System.Windows.Forms.PictureBox selectedIcon;
        private System.Windows.Forms.Button restoreOriginal;
        private System.Windows.Forms.Button themeSwitcher;
        private System.Windows.Forms.Label memoryStatus;
        private System.Windows.Forms.Label studioStatus;
        private System.Windows.Forms.Label errors;
        private System.Windows.Forms.CheckBox showModified;
    }
}