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
            this.label1 = new System.Windows.Forms.Label();
            this.enableIconOverride = new System.Windows.Forms.CheckBox();
            this.editIcon = new System.Windows.Forms.Button();
            this.selectedIcon = new System.Windows.Forms.PictureBox();
            this.restoreOriginal = new System.Windows.Forms.Button();
            this.themeSwitcher = new System.Windows.Forms.Button();
            this.memoryStatus = new System.Windows.Forms.Label();
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select Icon";
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
            this.editIcon.Enabled = false;
            this.editIcon.Location = new System.Drawing.Point(63, 229);
            this.editIcon.Name = "editIcon";
            this.editIcon.Size = new System.Drawing.Size(90, 23);
            this.editIcon.TabIndex = 5;
            this.editIcon.Text = "Edit Icon";
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
            this.memoryStatus.Size = new System.Drawing.Size(121, 13);
            this.memoryStatus.TabIndex = 8;
            this.memoryStatus.Text = "Current Memory Budget:";
            // 
            // ExplorerIconEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 281);
            this.Controls.Add(this.memoryStatus);
            this.Controls.Add(this.themeSwitcher);
            this.Controls.Add(this.restoreOriginal);
            this.Controls.Add(this.editIcon);
            this.Controls.Add(this.enableIconOverride);
            this.Controls.Add(this.selectedIcon);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox enableIconOverride;
        private System.Windows.Forms.Button editIcon;
        private System.Windows.Forms.PictureBox selectedIcon;
        private System.Windows.Forms.Button restoreOriginal;
        private System.Windows.Forms.Button themeSwitcher;
        private System.Windows.Forms.Label memoryStatus;
    }
}