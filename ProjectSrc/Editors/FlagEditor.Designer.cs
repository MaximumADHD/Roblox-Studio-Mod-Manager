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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabs = new System.Windows.Forms.TabControl();
            this.viewFlagsTab = new System.Windows.Forms.TabPage();
            this.overrideStatus = new System.Windows.Forms.Label();
            this.overrideSelected = new System.Windows.Forms.Button();
            this.flagSearchFilter = new System.Windows.Forms.TextBox();
            this.searchTitle = new System.Windows.Forms.Label();
            this.flagDataGridView = new System.Windows.Forms.DataGridView();
            this.overridesTab = new System.Windows.Forms.TabPage();
            this.removeAll = new System.Windows.Forms.Button();
            this.removeSelected = new System.Windows.Forms.Button();
            this.overrideDataGridView = new System.Windows.Forms.DataGridView();
            this.tabs.SuspendLayout();
            this.viewFlagsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flagDataGridView)).BeginInit();
            this.overridesTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.overrideDataGridView)).BeginInit();
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
            // overrideStatus
            // 
            this.overrideStatus.AutoSize = true;
            this.overrideStatus.Location = new System.Drawing.Point(182, 36);
            this.overrideStatus.Name = "overrideStatus";
            this.overrideStatus.Size = new System.Drawing.Size(0, 13);
            this.overrideStatus.TabIndex = 4;
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
            this.flagDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.flagDataGridView_CellFormatting);
            // 
            // overridesTab
            // 
            this.overridesTab.Controls.Add(this.removeAll);
            this.overridesTab.Controls.Add(this.removeSelected);
            this.overridesTab.Controls.Add(this.overrideDataGridView);
            this.overridesTab.Location = new System.Drawing.Point(4, 22);
            this.overridesTab.Name = "overridesTab";
            this.overridesTab.Padding = new System.Windows.Forms.Padding(3);
            this.overridesTab.Size = new System.Drawing.Size(454, 413);
            this.overridesTab.TabIndex = 1;
            this.overridesTab.Text = "Overrides";
            this.overridesTab.UseVisualStyleBackColor = true;
            // 
            // removeAll
            // 
            this.removeAll.Location = new System.Drawing.Point(229, 6);
            this.removeAll.Name = "removeAll";
            this.removeAll.Size = new System.Drawing.Size(217, 23);
            this.removeAll.TabIndex = 3;
            this.removeAll.Text = "Remove All";
            this.removeAll.UseVisualStyleBackColor = true;
            this.removeAll.Click += new System.EventHandler(this.removeAll_Click);
            // 
            // removeSelected
            // 
            this.removeSelected.Location = new System.Drawing.Point(6, 6);
            this.removeSelected.Name = "removeSelected";
            this.removeSelected.Size = new System.Drawing.Size(217, 23);
            this.removeSelected.TabIndex = 2;
            this.removeSelected.Text = "Remove Selected";
            this.removeSelected.UseVisualStyleBackColor = true;
            this.removeSelected.Click += new System.EventHandler(this.removeSelected_Click);
            // 
            // overrideDataGridView
            // 
            this.overrideDataGridView.AllowUserToAddRows = false;
            this.overrideDataGridView.AllowUserToDeleteRows = false;
            this.overrideDataGridView.AllowUserToResizeColumns = false;
            this.overrideDataGridView.AllowUserToResizeRows = false;
            this.overrideDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.overrideDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.overrideDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.overrideDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.overrideDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.overrideDataGridView.Location = new System.Drawing.Point(3, 35);
            this.overrideDataGridView.MultiSelect = false;
            this.overrideDataGridView.Name = "overrideDataGridView";
            this.overrideDataGridView.RowHeadersVisible = false;
            this.overrideDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Silver;
            this.overrideDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.overrideDataGridView.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.overrideDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.overrideDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.overrideDataGridView.Size = new System.Drawing.Size(448, 375);
            this.overrideDataGridView.TabIndex = 1;
            this.overrideDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellEndEdit);
            this.overrideDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellMouseEnter);
            this.overrideDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellMouseLeave);
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
            this.Load += new System.EventHandler(this.FlagEditor_Load);
            this.tabs.ResumeLayout(false);
            this.viewFlagsTab.ResumeLayout(false);
            this.viewFlagsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.flagDataGridView)).EndInit();
            this.overridesTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.overrideDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage viewFlagsTab;
        private System.Windows.Forms.TabPage overridesTab;
        private System.Windows.Forms.DataGridView flagDataGridView;
        private System.Windows.Forms.TextBox flagSearchFilter;
        private System.Windows.Forms.Label searchTitle;
        private System.Windows.Forms.Button overrideSelected;
        private System.Windows.Forms.DataGridView overrideDataGridView;
        private System.Windows.Forms.Label overrideStatus;
        private System.Windows.Forms.Button removeAll;
        private System.Windows.Forms.Button removeSelected;
    }
}