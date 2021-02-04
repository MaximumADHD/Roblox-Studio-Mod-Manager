namespace RobloxStudioModManager
{
    partial class ClassIconEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClassIconEditor));
            this.iconContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.header = new System.Windows.Forms.Label();
            this.enableIconOverride = new System.Windows.Forms.CheckBox();
            this.selectedIcon = new System.Windows.Forms.PictureBox();
            this.restoreOriginal = new System.Windows.Forms.Button();
            this.themeSwitcher = new System.Windows.Forms.Button();
            this.errors = new System.Windows.Forms.Label();
            this.showModified = new System.Windows.Forms.CheckBox();
            this.openIconFolder = new System.Windows.Forms.Button();
            this.iconSlotsTitle = new System.Windows.Forms.Label();
            this.itemSlots = new System.Windows.Forms.NumericUpDown();
            this.editIcon = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.selectedIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemSlots)).BeginInit();
            this.SuspendLayout();
            // 
            // iconContainer
            // 
            this.iconContainer.AutoScroll = true;
            this.iconContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.iconContainer.Location = new System.Drawing.Point(20, 54);
            this.iconContainer.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.iconContainer.MaximumSize = new System.Drawing.Size(451, 19225);
            this.iconContainer.Name = "iconContainer";
            this.iconContainer.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.iconContainer.Size = new System.Drawing.Size(421, 314);
            this.iconContainer.TabIndex = 0;
            // 
            // header
            // 
            this.header.AutoSize = true;
            this.header.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.header.Location = new System.Drawing.Point(13, 8);
            this.header.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(138, 32);
            this.header.TabIndex = 2;
            this.header.Text = "Initializing...";
            // 
            // enableIconOverride
            // 
            this.enableIconOverride.AutoSize = true;
            this.enableIconOverride.Location = new System.Drawing.Point(110, 425);
            this.enableIconOverride.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.enableIconOverride.Name = "enableIconOverride";
            this.enableIconOverride.Size = new System.Drawing.Size(145, 29);
            this.enableIconOverride.TabIndex = 4;
            this.enableIconOverride.Text = "Override Icon";
            this.enableIconOverride.UseVisualStyleBackColor = true;
            this.enableIconOverride.CheckedChanged += new System.EventHandler(this.enableIconOverride_CheckedChanged);
            // 
            // selectedIcon
            // 
            this.selectedIcon.BackColor = System.Drawing.Color.White;
            this.selectedIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.selectedIcon.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.selectedIcon.Location = new System.Drawing.Point(20, 380);
            this.selectedIcon.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.selectedIcon.Name = "selectedIcon";
            this.selectedIcon.Size = new System.Drawing.Size(84, 84);
            this.selectedIcon.TabIndex = 3;
            this.selectedIcon.TabStop = false;
            // 
            // restoreOriginal
            // 
            this.restoreOriginal.Enabled = false;
            this.restoreOriginal.Location = new System.Drawing.Point(293, 423);
            this.restoreOriginal.Margin = new System.Windows.Forms.Padding(2);
            this.restoreOriginal.Name = "restoreOriginal";
            this.restoreOriginal.Size = new System.Drawing.Size(148, 31);
            this.restoreOriginal.TabIndex = 6;
            this.restoreOriginal.Text = "Restore Original";
            this.restoreOriginal.UseCompatibleTextRendering = true;
            this.restoreOriginal.UseVisualStyleBackColor = true;
            this.restoreOriginal.Click += new System.EventHandler(this.restoreOriginal_Click);
            // 
            // themeSwitcher
            // 
            this.themeSwitcher.Location = new System.Drawing.Point(160, 515);
            this.themeSwitcher.Margin = new System.Windows.Forms.Padding(2);
            this.themeSwitcher.Name = "themeSwitcher";
            this.themeSwitcher.Size = new System.Drawing.Size(281, 31);
            this.themeSwitcher.TabIndex = 7;
            this.themeSwitcher.Text = "Theme: Light";
            this.themeSwitcher.UseCompatibleTextRendering = true;
            this.themeSwitcher.UseVisualStyleBackColor = true;
            this.themeSwitcher.Click += new System.EventHandler(this.themeSwitcher_Click);
            // 
            // errors
            // 
            this.errors.AutoSize = true;
            this.errors.ForeColor = System.Drawing.Color.Red;
            this.errors.Location = new System.Drawing.Point(15, 558);
            this.errors.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.errors.Name = "errors";
            this.errors.Size = new System.Drawing.Size(0, 25);
            this.errors.TabIndex = 10;
            // 
            // showModified
            // 
            this.showModified.AutoSize = true;
            this.showModified.Location = new System.Drawing.Point(110, 390);
            this.showModified.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.showModified.Name = "showModified";
            this.showModified.Size = new System.Drawing.Size(176, 29);
            this.showModified.TabIndex = 11;
            this.showModified.Text = "Show Overridden";
            this.showModified.UseVisualStyleBackColor = true;
            this.showModified.CheckedChanged += new System.EventHandler(this.showModified_CheckedChanged);
            // 
            // openIconFolder
            // 
            this.openIconFolder.Location = new System.Drawing.Point(160, 476);
            this.openIconFolder.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.openIconFolder.Name = "openIconFolder";
            this.openIconFolder.Size = new System.Drawing.Size(281, 31);
            this.openIconFolder.TabIndex = 12;
            this.openIconFolder.Text = "Open Icon Folder";
            this.openIconFolder.UseVisualStyleBackColor = true;
            this.openIconFolder.Click += new System.EventHandler(this.openIconFolder_Click);
            // 
            // iconSlotsTitle
            // 
            this.iconSlotsTitle.AutoSize = true;
            this.iconSlotsTitle.Location = new System.Drawing.Point(15, 484);
            this.iconSlotsTitle.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.iconSlotsTitle.Name = "iconSlotsTitle";
            this.iconSlotsTitle.Size = new System.Drawing.Size(137, 25);
            this.iconSlotsTitle.TabIndex = 13;
            this.iconSlotsTitle.Text = "Extra Icon Slots:";
            // 
            // itemSlots
            // 
            this.itemSlots.Location = new System.Drawing.Point(20, 515);
            this.itemSlots.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.itemSlots.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.itemSlots.Name = "itemSlots";
            this.itemSlots.Size = new System.Drawing.Size(130, 31);
            this.itemSlots.TabIndex = 14;
            this.itemSlots.ValueChanged += new System.EventHandler(this.itemSlots_ValueChanged);
            this.itemSlots.KeyDown += new System.Windows.Forms.KeyEventHandler(this.itemSlots_KeyDown);
            // 
            // editIcon
            // 
            this.editIcon.Location = new System.Drawing.Point(293, 388);
            this.editIcon.Margin = new System.Windows.Forms.Padding(2);
            this.editIcon.Name = "editIcon";
            this.editIcon.Size = new System.Drawing.Size(148, 31);
            this.editIcon.TabIndex = 5;
            this.editIcon.Text = "Edit Icon 0";
            this.editIcon.UseVisualStyleBackColor = true;
            this.editIcon.Click += new System.EventHandler(this.editIcon_Click);
            // 
            // ClassIconEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(463, 570);
            this.Controls.Add(this.itemSlots);
            this.Controls.Add(this.iconSlotsTitle);
            this.Controls.Add(this.openIconFolder);
            this.Controls.Add(this.showModified);
            this.Controls.Add(this.errors);
            this.Controls.Add(this.themeSwitcher);
            this.Controls.Add(this.restoreOriginal);
            this.Controls.Add(this.editIcon);
            this.Controls.Add(this.enableIconOverride);
            this.Controls.Add(this.selectedIcon);
            this.Controls.Add(this.header);
            this.Controls.Add(this.iconContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = RobloxStudioModManager.Properties.Resources.Icon;
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.MaximizeBox = false;
            this.Name = "ClassIconEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Class Icon Editor";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ClassIconEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.selectedIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemSlots)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel iconContainer;
        private System.Windows.Forms.Label header;
        private System.Windows.Forms.CheckBox enableIconOverride;
        private System.Windows.Forms.PictureBox selectedIcon;
        private System.Windows.Forms.Button restoreOriginal;
        private System.Windows.Forms.Button themeSwitcher;
        private System.Windows.Forms.Label errors;
        private System.Windows.Forms.CheckBox showModified;
        private System.Windows.Forms.Button openIconFolder;
        private System.Windows.Forms.Label iconSlotsTitle;
        private System.Windows.Forms.NumericUpDown itemSlots;
        private System.Windows.Forms.Button editIcon;
    }
}