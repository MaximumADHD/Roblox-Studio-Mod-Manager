namespace RobloxStudioModManager
{
    partial class FlagEditor
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlagEditor));
            this.tabs = new System.Windows.Forms.TabControl();
            this.viewFlagsTab = new System.Windows.Forms.TabPage();
            this.overrideStatus = new System.Windows.Forms.Label();
            this.overrideSelected = new System.Windows.Forms.Button();
            this.flagSearchFilter = new System.Windows.Forms.TextBox();
            this.searchTitle = new System.Windows.Forms.Label();
            this.flagDataGridView = new System.Windows.Forms.DataGridView();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.overridesTab = new System.Windows.Forms.TabPage();
            this.removeAll = new System.Windows.Forms.Button();
            this.removeSelected = new System.Windows.Forms.Button();
            this.overrideDataGridView = new System.Windows.Forms.DataGridView();
            this.overrideNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.overrideTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.initialOverrideValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.tabs.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(770, 844);
            this.tabs.TabIndex = 0;
            // 
            // viewFlagsTab
            // 
            this.viewFlagsTab.Controls.Add(this.overrideStatus);
            this.viewFlagsTab.Controls.Add(this.overrideSelected);
            this.viewFlagsTab.Controls.Add(this.flagSearchFilter);
            this.viewFlagsTab.Controls.Add(this.searchTitle);
            this.viewFlagsTab.Controls.Add(this.flagDataGridView);
            this.viewFlagsTab.Location = new System.Drawing.Point(4, 34);
            this.viewFlagsTab.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.viewFlagsTab.Name = "viewFlagsTab";
            this.viewFlagsTab.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.viewFlagsTab.Size = new System.Drawing.Size(762, 806);
            this.viewFlagsTab.TabIndex = 0;
            this.viewFlagsTab.Text = "View Flags";
            this.viewFlagsTab.UseVisualStyleBackColor = true;
            // 
            // overrideStatus
            // 
            this.overrideStatus.AutoSize = true;
            this.overrideStatus.Location = new System.Drawing.Point(303, 69);
            this.overrideStatus.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.overrideStatus.Name = "overrideStatus";
            this.overrideStatus.Size = new System.Drawing.Size(0, 25);
            this.overrideStatus.TabIndex = 4;
            // 
            // overrideSelected
            // 
            this.overrideSelected.Location = new System.Drawing.Point(98, 56);
            this.overrideSelected.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.overrideSelected.Name = "overrideSelected";
            this.overrideSelected.Size = new System.Drawing.Size(195, 38);
            this.overrideSelected.TabIndex = 3;
            this.overrideSelected.Text = "Override Selected";
            this.overrideSelected.UseVisualStyleBackColor = true;
            this.overrideSelected.Click += new System.EventHandler(this.overrideSelected_Click);
            // 
            // flagSearchFilter
            // 
            this.flagSearchFilter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.flagSearchFilter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.flagSearchFilter.Location = new System.Drawing.Point(98, 19);
            this.flagSearchFilter.Margin = new System.Windows.Forms.Padding(2, 6, 5, 6);
            this.flagSearchFilter.Name = "flagSearchFilter";
            this.flagSearchFilter.Size = new System.Drawing.Size(642, 31);
            this.flagSearchFilter.TabIndex = 2;
            this.flagSearchFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.flagSearchFilter_KeyDown);
            this.flagSearchFilter.Leave += new System.EventHandler(this.flagSearchFilter_Leave);
            // 
            // searchTitle
            // 
            this.searchTitle.AutoSize = true;
            this.searchTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.searchTitle.Location = new System.Drawing.Point(11, 18);
            this.searchTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.searchTitle.Name = "searchTitle";
            this.searchTitle.Size = new System.Drawing.Size(83, 30);
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
            this.flagDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.typeColumn,
            this.valueColumn});
            this.flagDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flagDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.flagDataGridView.Location = new System.Drawing.Point(5, 106);
            this.flagDataGridView.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.flagDataGridView.MultiSelect = false;
            this.flagDataGridView.Name = "flagDataGridView";
            this.flagDataGridView.RowHeadersVisible = false;
            this.flagDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.flagDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.flagDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.flagDataGridView.Size = new System.Drawing.Size(752, 694);
            this.flagDataGridView.TabIndex = 0;
            this.flagDataGridView.VirtualMode = true;
            this.flagDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.flagDataGridView_CellFormatting);
            this.flagDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.flagDataGridView_CellValueNeeded);
            // 
            // nameColumn
            // 
            this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameColumn.FillWeight = 250F;
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.MinimumWidth = 8;
            this.nameColumn.Name = "nameColumn";
            this.nameColumn.ReadOnly = true;
            this.nameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.nameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // typeColumn
            // 
            this.typeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.typeColumn.FillWeight = 50F;
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.MinimumWidth = 8;
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.ReadOnly = true;
            this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // valueColumn
            // 
            this.valueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.valueColumn.FillWeight = 150F;
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.MinimumWidth = 8;
            this.valueColumn.Name = "valueColumn";
            this.valueColumn.ReadOnly = true;
            this.valueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // overridesTab
            // 
            this.overridesTab.Controls.Add(this.removeAll);
            this.overridesTab.Controls.Add(this.removeSelected);
            this.overridesTab.Controls.Add(this.overrideDataGridView);
            this.overridesTab.Location = new System.Drawing.Point(4, 34);
            this.overridesTab.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.overridesTab.Name = "overridesTab";
            this.overridesTab.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.overridesTab.Size = new System.Drawing.Size(762, 806);
            this.overridesTab.TabIndex = 1;
            this.overridesTab.Text = "Overrides";
            this.overridesTab.UseVisualStyleBackColor = true;
            // 
            // removeAll
            // 
            this.removeAll.Location = new System.Drawing.Point(382, 12);
            this.removeAll.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.removeAll.Name = "removeAll";
            this.removeAll.Size = new System.Drawing.Size(362, 44);
            this.removeAll.TabIndex = 3;
            this.removeAll.Text = "Remove All";
            this.removeAll.UseVisualStyleBackColor = true;
            this.removeAll.Click += new System.EventHandler(this.removeAll_Click);
            // 
            // removeSelected
            // 
            this.removeSelected.Location = new System.Drawing.Point(10, 12);
            this.removeSelected.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.removeSelected.Name = "removeSelected";
            this.removeSelected.Size = new System.Drawing.Size(362, 44);
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
            this.overrideDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.overrideNameColumn,
            this.overrideTypeColumn,
            this.initialOverrideValueColumn});
            this.overrideDataGridView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.overrideDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.overrideDataGridView.Location = new System.Drawing.Point(5, 79);
            this.overrideDataGridView.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.overrideDataGridView.MultiSelect = false;
            this.overrideDataGridView.Name = "overrideDataGridView";
            this.overrideDataGridView.RowHeadersVisible = false;
            this.overrideDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Silver;
            this.overrideDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.overrideDataGridView.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.overrideDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.overrideDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.overrideDataGridView.Size = new System.Drawing.Size(752, 721);
            this.overrideDataGridView.TabIndex = 1;
            this.overrideDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellEndEdit);
            this.overrideDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellMouseEnter);
            this.overrideDataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.overrideDataGridView_CellMouseLeave);
            // 
            // overrideNameColumn
            // 
            this.overrideNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.overrideNameColumn.DataPropertyName = "Name";
            this.overrideNameColumn.FillWeight = 150F;
            this.overrideNameColumn.HeaderText = "Name";
            this.overrideNameColumn.MinimumWidth = 8;
            this.overrideNameColumn.Name = "overrideNameColumn";
            this.overrideNameColumn.ReadOnly = true;
            this.overrideNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // overrideTypeColumn
            // 
            this.overrideTypeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.overrideTypeColumn.DataPropertyName = "Type";
            this.overrideTypeColumn.FillWeight = 40F;
            this.overrideTypeColumn.HeaderText = "Type";
            this.overrideTypeColumn.MinimumWidth = 8;
            this.overrideTypeColumn.Name = "overrideTypeColumn";
            this.overrideTypeColumn.ReadOnly = true;
            this.overrideTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // initialOverrideValueColumn
            // 
            this.initialOverrideValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.initialOverrideValueColumn.DataPropertyName = "Value";
            this.initialOverrideValueColumn.HeaderText = "Value";
            this.initialOverrideValueColumn.MinimumWidth = 8;
            this.initialOverrideValueColumn.Name = "initialOverrideValueColumn";
            this.initialOverrideValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // FlagEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(770, 844);
            this.Controls.Add(this.tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
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
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn overrideNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn overrideTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn initialOverrideValueColumn;
    }
}