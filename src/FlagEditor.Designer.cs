namespace RobloxStudioModManager
{
    partial class FlagEditor
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
            this.tabs = new System.Windows.Forms.TabControl();
            this.viewFlagsTab = new System.Windows.Forms.TabPage();
            this.flagSearchFilter = new System.Windows.Forms.TextBox();
            this.searchTitle = new System.Windows.Forms.Label();
            this.flagDataGridView = new System.Windows.Forms.DataGridView();
            this.overridesTab = new System.Windows.Forms.TabPage();
            this.overrideSelected = new System.Windows.Forms.Button();
            this.overrideStatus = new System.Windows.Forms.Label();
            this.tabs.SuspendLayout();
            this.viewFlagsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flagDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.viewFlagsTab);
            this.tabs.Controls.Add(this.overridesTab);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(462, 439);
            this.tabs.TabIndex = 0;
            // 
            // viewFlagsTab
            // 
            this.viewFlagsTab.Controls.Add(this.overrideStatus);
            this.viewFlagsTab.Controls.Add(this.overrideSelected);
            this.viewFlagsTab.Controls.Add(this.flagSearchFilter);
            this.viewFlagsTab.Controls.Add(this.searchTitle);
            this.viewFlagsTab.Controls.Add(this.flagDataGridView);
            this.viewFlagsTab.Location = new System.Drawing.Point(4, 22);
            this.viewFlagsTab.Name = "viewFlagsTab";
            this.viewFlagsTab.Padding = new System.Windows.Forms.Padding(3);
            this.viewFlagsTab.Size = new System.Drawing.Size(454, 413);
            this.viewFlagsTab.TabIndex = 0;
            this.viewFlagsTab.Text = "View Flags";
            this.viewFlagsTab.UseVisualStyleBackColor = true;
            // 
            // flagSearchFilter
            // 
            this.flagSearchFilter.Location = new System.Drawing.Point(59, 6);
            this.flagSearchFilter.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.flagSearchFilter.Name = "flagSearchFilter";
            this.flagSearchFilter.Size = new System.Drawing.Size(387, 20);
            this.flagSearchFilter.TabIndex = 2;
            this.flagSearchFilter.TextChanged += new System.EventHandler(this.flagSearchFilter_TextChanged);
            this.flagSearchFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.flagSearchFilter_KeyDown);
            // 
            // searchTitle
            // 
            this.searchTitle.AutoSize = true;
            this.searchTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchTitle.Location = new System.Drawing.Point(6, 9);
            this.searchTitle.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.searchTitle.Name = "searchTitle";
            this.searchTitle.Size = new System.Drawing.Size(44, 13);
            this.searchTitle.TabIndex = 1;
            this.searchTitle.Text = "Search:";
            // 
            // flagDataGridView
            // 
            this.flagDataGridView.AllowUserToAddRows = false;
            this.flagDataGridView.AllowUserToDeleteRows = false;
            this.flagDataGridView.AllowUserToResizeColumns = false;
            this.flagDataGridView.AllowUserToResizeRows = false;
            this.flagDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.flagDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.flagDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.flagDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flagDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.flagDataGridView.Location = new System.Drawing.Point(3, 58);
            this.flagDataGridView.MultiSelect = false;
            this.flagDataGridView.Name = "flagDataGridView";
            this.flagDataGridView.RowHeadersVisible = false;
            this.flagDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.flagDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.flagDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.flagDataGridView.Size = new System.Drawing.Size(448, 352);
            this.flagDataGridView.TabIndex = 0;
            this.flagDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.flagDataGridView_CellContentClick);
            // 
            // overridesTab
            // 
            this.overridesTab.Location = new System.Drawing.Point(4, 22);
            this.overridesTab.Name = "overridesTab";
            this.overridesTab.Padding = new System.Windows.Forms.Padding(3);
            this.overridesTab.Size = new System.Drawing.Size(454, 413);
            this.overridesTab.TabIndex = 1;
            this.overridesTab.Text = "Overrides";
            this.overridesTab.UseVisualStyleBackColor = true;
            // 
            // overrideSelected
            // 
            this.overrideSelected.Location = new System.Drawing.Point(59, 32);
            this.overrideSelected.Name = "overrideSelected";
            this.overrideSelected.Size = new System.Drawing.Size(117, 20);
            this.overrideSelected.TabIndex = 3;
            this.overrideSelected.Text = "Override Selected";
            this.overrideSelected.UseVisualStyleBackColor = true;
            this.overrideSelected.Click += new System.EventHandler(this.overrideSelected_Click);
            // 
            // overrideStatus
            // 
            this.overrideStatus.AutoSize = true;
            this.overrideStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.overrideStatus.Location = new System.Drawing.Point(180, 36);
            this.overrideStatus.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.overrideStatus.Name = "overrideStatus";
            this.overrideStatus.Size = new System.Drawing.Size(114, 13);
            this.overrideStatus.TabIndex = 4;
            this.overrideStatus.Text = "Initializing Flag Editor...";
            // 
            // FlagEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 439);
            this.Controls.Add(this.tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RobloxStudioModManager.Properties.Resources.Icon;
            this.MaximizeBox = false;
            this.Name = "FlagEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flag Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FlagEditor_FormClosed);
            this.Load += new System.EventHandler(this.FlagEditor_Load);
            this.tabs.ResumeLayout(false);
            this.viewFlagsTab.ResumeLayout(false);
            this.viewFlagsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flagDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage viewFlagsTab;
        private System.Windows.Forms.TabPage overridesTab;
        private System.Windows.Forms.DataGridView flagDataGridView;
        private System.Windows.Forms.TextBox flagSearchFilter;
        private System.Windows.Forms.Label searchTitle;
        private System.Windows.Forms.Label overrideStatus;
        private System.Windows.Forms.Button overrideSelected;
    }
}