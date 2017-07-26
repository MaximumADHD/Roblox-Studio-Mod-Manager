namespace RobloxModManager
{
    partial class FVariableEditor
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
            this.fvarConfig = new System.Windows.Forms.CheckedListBox();
            this.availableFVariables = new System.Windows.Forms.ListBox();
            this.add = new System.Windows.Forms.Button();
            this.remove = new System.Windows.Forms.Button();
            this.availableTitle = new System.Windows.Forms.Label();
            this.configTitle = new System.Windows.Forms.Label();
            this.searchFilter = new System.Windows.Forms.TextBox();
            this.searchFilterLbl = new System.Windows.Forms.Label();
            this.fvarConfigTabs = new System.Windows.Forms.TabControl();
            this.basicFVariableConfig = new System.Windows.Forms.TabPage();
            this.advFVariableConfig = new System.Windows.Forms.TabPage();
            this.advFVarSelect = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.fVarNote = new System.Windows.Forms.Label();
            this.advFVarValue = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.advFVarType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.fvarConfigTabs.SuspendLayout();
            this.basicFVariableConfig.SuspendLayout();
            this.advFVariableConfig.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fvarConfig
            // 
            this.fvarConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fvarConfig.FormattingEnabled = true;
            this.fvarConfig.Location = new System.Drawing.Point(3, 3);
            this.fvarConfig.Name = "fvarConfig";
            this.fvarConfig.Size = new System.Drawing.Size(170, 180);
            this.fvarConfig.TabIndex = 1;
            this.fvarConfig.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.fvarConfig_ItemCheck);
            this.fvarConfig.SelectedIndexChanged += new System.EventHandler(this.fvarConfig_SelectedIndexChanged);
            // 
            // availableFVariables
            // 
            this.availableFVariables.FormattingEnabled = true;
            this.availableFVariables.Location = new System.Drawing.Point(12, 32);
            this.availableFVariables.Name = "availableFVariables";
            this.availableFVariables.Size = new System.Drawing.Size(184, 212);
            this.availableFVariables.Sorted = true;
            this.availableFVariables.TabIndex = 2;
            this.availableFVariables.SelectedIndexChanged += new System.EventHandler(this.availableFVariables_SelectedIndexChanged);
            // 
            // add
            // 
            this.add.Enabled = false;
            this.add.Location = new System.Drawing.Point(12, 250);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(184, 23);
            this.add.TabIndex = 3;
            this.add.Text = "Add";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // remove
            // 
            this.remove.Enabled = false;
            this.remove.Location = new System.Drawing.Point(214, 250);
            this.remove.Name = "remove";
            this.remove.Size = new System.Drawing.Size(184, 23);
            this.remove.TabIndex = 4;
            this.remove.Text = "Remove";
            this.remove.UseVisualStyleBackColor = true;
            this.remove.Click += new System.EventHandler(this.remove_Click);
            // 
            // availableTitle
            // 
            this.availableTitle.AutoSize = true;
            this.availableTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.availableTitle.Location = new System.Drawing.Point(9, 9);
            this.availableTitle.Name = "availableTitle";
            this.availableTitle.Size = new System.Drawing.Size(126, 13);
            this.availableTitle.TabIndex = 5;
            this.availableTitle.Text = "Available FVariables:";
            // 
            // configTitle
            // 
            this.configTitle.AutoSize = true;
            this.configTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configTitle.Location = new System.Drawing.Point(215, 9);
            this.configTitle.Name = "configTitle";
            this.configTitle.Size = new System.Drawing.Size(143, 13);
            this.configTitle.TabIndex = 6;
            this.configTitle.Text = "FVariable Configuration:";
            // 
            // searchFilter
            // 
            this.searchFilter.Location = new System.Drawing.Point(12, 292);
            this.searchFilter.Name = "searchFilter";
            this.searchFilter.Size = new System.Drawing.Size(386, 20);
            this.searchFilter.TabIndex = 7;
            this.searchFilter.TextChanged += new System.EventHandler(this.reassembleListings);
            // 
            // searchFilterLbl
            // 
            this.searchFilterLbl.AutoSize = true;
            this.searchFilterLbl.Location = new System.Drawing.Point(9, 276);
            this.searchFilterLbl.Name = "searchFilterLbl";
            this.searchFilterLbl.Size = new System.Drawing.Size(69, 13);
            this.searchFilterLbl.TabIndex = 8;
            this.searchFilterLbl.Text = "Search Filter:";
            // 
            // fvarConfigTabs
            // 
            this.fvarConfigTabs.Controls.Add(this.basicFVariableConfig);
            this.fvarConfigTabs.Controls.Add(this.advFVariableConfig);
            this.fvarConfigTabs.Location = new System.Drawing.Point(214, 32);
            this.fvarConfigTabs.Name = "fvarConfigTabs";
            this.fvarConfigTabs.SelectedIndex = 0;
            this.fvarConfigTabs.Size = new System.Drawing.Size(184, 212);
            this.fvarConfigTabs.TabIndex = 9;
            this.fvarConfigTabs.Selected += new System.Windows.Forms.TabControlEventHandler(this.reassembleListings);
            // 
            // basicFVariableConfig
            // 
            this.basicFVariableConfig.Controls.Add(this.fvarConfig);
            this.basicFVariableConfig.Location = new System.Drawing.Point(4, 22);
            this.basicFVariableConfig.Name = "basicFVariableConfig";
            this.basicFVariableConfig.Padding = new System.Windows.Forms.Padding(3);
            this.basicFVariableConfig.Size = new System.Drawing.Size(176, 186);
            this.basicFVariableConfig.TabIndex = 0;
            this.basicFVariableConfig.Text = "Basic";
            this.basicFVariableConfig.UseVisualStyleBackColor = true;
            // 
            // advFVariableConfig
            // 
            this.advFVariableConfig.Controls.Add(this.advFVarSelect);
            this.advFVariableConfig.Controls.Add(this.label3);
            this.advFVariableConfig.Controls.Add(this.fVarNote);
            this.advFVariableConfig.Controls.Add(this.advFVarValue);
            this.advFVariableConfig.Controls.Add(this.label2);
            this.advFVariableConfig.Controls.Add(this.advFVarType);
            this.advFVariableConfig.Controls.Add(this.label1);
            this.advFVariableConfig.Location = new System.Drawing.Point(4, 22);
            this.advFVariableConfig.Name = "advFVariableConfig";
            this.advFVariableConfig.Padding = new System.Windows.Forms.Padding(3);
            this.advFVariableConfig.Size = new System.Drawing.Size(176, 186);
            this.advFVariableConfig.TabIndex = 1;
            this.advFVariableConfig.Text = "Advanced";
            this.advFVariableConfig.UseVisualStyleBackColor = true;
            // 
            // advFVarSelect
            // 
            this.advFVarSelect.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.advFVarSelect.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.advFVarSelect.FormattingEnabled = true;
            this.advFVarSelect.Items.AddRange(new object[] {
            "Flag",
            "Int",
            "Log",
            "String"});
            this.advFVarSelect.Location = new System.Drawing.Point(6, 80);
            this.advFVarSelect.Name = "advFVarSelect";
            this.advFVarSelect.Size = new System.Drawing.Size(164, 21);
            this.advFVarSelect.TabIndex = 9;
            this.advFVarSelect.TextChanged += new System.EventHandler(this.advFVarSelect_TextChanged);
            this.advFVarSelect.Enter += new System.EventHandler(this.advFVarSelect_Enter);
            this.advFVarSelect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.advFVarSelect_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "FVariable:";
            // 
            // fVarNote
            // 
            this.fVarNote.AutoSize = true;
            this.fVarNote.Location = new System.Drawing.Point(3, 3);
            this.fVarNote.Name = "fVarNote";
            this.fVarNote.Size = new System.Drawing.Size(161, 52);
            this.fVarNote.TabIndex = 7;
            this.fVarNote.Text = "Make sure you edit this carefully.\r\nYour configuration may not work\r\ncorrectly if" +
    " you enter an invalid \r\nvalue.";
            // 
            // advFVarValue
            // 
            this.advFVarValue.Enabled = false;
            this.advFVarValue.Location = new System.Drawing.Point(6, 160);
            this.advFVarValue.Name = "advFVarValue";
            this.advFVarValue.Size = new System.Drawing.Size(164, 20);
            this.advFVarValue.TabIndex = 6;
            this.advFVarValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.advFVarValue_KeyDown);
            this.advFVarValue.Leave += new System.EventHandler(this.advFVarValue_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "FVariable Value:";
            // 
            // advFVarType
            // 
            this.advFVarType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.advFVarType.Enabled = false;
            this.advFVarType.FormattingEnabled = true;
            this.advFVarType.Items.AddRange(new object[] {
            "Flag",
            "Int",
            "Log",
            "String"});
            this.advFVarType.Location = new System.Drawing.Point(6, 120);
            this.advFVarType.Name = "advFVarType";
            this.advFVarType.Size = new System.Drawing.Size(164, 21);
            this.advFVarType.TabIndex = 3;
            this.advFVarType.SelectedIndexChanged += new System.EventHandler(this.advFVarType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "FVariable Type:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLbl});
            this.statusStrip1.Location = new System.Drawing.Point(0, 321);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(410, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLbl
            // 
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(0, 17);
            this.statusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 321);
            this.splitter1.TabIndex = 11;
            this.splitter1.TabStop = false;
            // 
            // FVariableEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 343);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.fvarConfigTabs);
            this.Controls.Add(this.searchFilterLbl);
            this.Controls.Add(this.searchFilter);
            this.Controls.Add(this.configTitle);
            this.Controls.Add(this.availableTitle);
            this.Controls.Add(this.remove);
            this.Controls.Add(this.add);
            this.Controls.Add(this.availableFVariables);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FVariableEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FVariable Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FVariableEditor_FormClosed);
            this.Load += new System.EventHandler(this.FVariableEditor_Load);
            this.fvarConfigTabs.ResumeLayout(false);
            this.basicFVariableConfig.ResumeLayout(false);
            this.advFVariableConfig.ResumeLayout(false);
            this.advFVariableConfig.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckedListBox fvarConfig;
        private System.Windows.Forms.ListBox availableFVariables;
        private System.Windows.Forms.Button add;
        private System.Windows.Forms.Button remove;
        private System.Windows.Forms.Label availableTitle;
        private System.Windows.Forms.Label configTitle;
        private System.Windows.Forms.TextBox searchFilter;
        private System.Windows.Forms.Label searchFilterLbl;
        private System.Windows.Forms.TabControl fvarConfigTabs;
        private System.Windows.Forms.TabPage basicFVariableConfig;
        private System.Windows.Forms.TabPage advFVariableConfig;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLbl;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ComboBox advFVarType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox advFVarValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label fVarNote;
        private System.Windows.Forms.ComboBox advFVarSelect;
        private System.Windows.Forms.Label label3;
    }
}