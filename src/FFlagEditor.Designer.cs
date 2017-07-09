namespace RobloxModManager
{
    partial class FFlagEditor
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
            this.statusLbl = new System.Windows.Forms.Label();
            this.fflagConfig = new System.Windows.Forms.CheckedListBox();
            this.availableFFlags = new System.Windows.Forms.ListBox();
            this.add = new System.Windows.Forms.Button();
            this.remove = new System.Windows.Forms.Button();
            this.availableTitle = new System.Windows.Forms.Label();
            this.configTitle = new System.Windows.Forms.Label();
            this.searchFilter = new System.Windows.Forms.TextBox();
            this.searchFilterLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // statusLbl
            // 
            this.statusLbl.AutoSize = true;
            this.statusLbl.Location = new System.Drawing.Point(9, 277);
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(0, 13);
            this.statusLbl.TabIndex = 0;
            // 
            // fflagConfig
            // 
            this.fflagConfig.FormattingEnabled = true;
            this.fflagConfig.Location = new System.Drawing.Point(173, 35);
            this.fflagConfig.Name = "fflagConfig";
            this.fflagConfig.Size = new System.Drawing.Size(149, 154);
            this.fflagConfig.TabIndex = 1;
            this.fflagConfig.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.fflagConfig_ItemCheck);
            this.fflagConfig.SelectedIndexChanged += new System.EventHandler(this.fflagConfig_SelectedIndexChanged);
            // 
            // availableFFlags
            // 
            this.availableFFlags.FormattingEnabled = true;
            this.availableFFlags.Location = new System.Drawing.Point(12, 32);
            this.availableFFlags.Name = "availableFFlags";
            this.availableFFlags.Size = new System.Drawing.Size(149, 160);
            this.availableFFlags.Sorted = true;
            this.availableFFlags.TabIndex = 2;
            this.availableFFlags.SelectedIndexChanged += new System.EventHandler(this.availableFFlags_SelectedIndexChanged);
            // 
            // add
            // 
            this.add.Enabled = false;
            this.add.Location = new System.Drawing.Point(12, 198);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(149, 23);
            this.add.TabIndex = 3;
            this.add.Text = "Add";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // remove
            // 
            this.remove.Enabled = false;
            this.remove.Location = new System.Drawing.Point(173, 198);
            this.remove.Name = "remove";
            this.remove.Size = new System.Drawing.Size(149, 23);
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
            this.availableTitle.Size = new System.Drawing.Size(104, 13);
            this.availableTitle.TabIndex = 5;
            this.availableTitle.Text = "Available FFlags:";
            // 
            // configTitle
            // 
            this.configTitle.AutoSize = true;
            this.configTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configTitle.Location = new System.Drawing.Point(170, 9);
            this.configTitle.Name = "configTitle";
            this.configTitle.Size = new System.Drawing.Size(121, 13);
            this.configTitle.TabIndex = 6;
            this.configTitle.Text = "FFlag Configuration:";
            // 
            // searchFilter
            // 
            this.searchFilter.Location = new System.Drawing.Point(12, 245);
            this.searchFilter.Name = "searchFilter";
            this.searchFilter.Size = new System.Drawing.Size(310, 20);
            this.searchFilter.TabIndex = 7;
            this.searchFilter.TextChanged += new System.EventHandler(this.searchFilter_TextChanged);
            // 
            // searchFilterLbl
            // 
            this.searchFilterLbl.AutoSize = true;
            this.searchFilterLbl.Location = new System.Drawing.Point(9, 229);
            this.searchFilterLbl.Name = "searchFilterLbl";
            this.searchFilterLbl.Size = new System.Drawing.Size(69, 13);
            this.searchFilterLbl.TabIndex = 8;
            this.searchFilterLbl.Text = "Search Filter:";
            // 
            // FFlagEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 299);
            this.Controls.Add(this.searchFilterLbl);
            this.Controls.Add(this.searchFilter);
            this.Controls.Add(this.configTitle);
            this.Controls.Add(this.availableTitle);
            this.Controls.Add(this.remove);
            this.Controls.Add(this.add);
            this.Controls.Add(this.availableFFlags);
            this.Controls.Add(this.fflagConfig);
            this.Controls.Add(this.statusLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FFlagEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FFlag Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FFlagEditor_FormClosed);
            this.Load += new System.EventHandler(this.FFlagEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label statusLbl;
        private System.Windows.Forms.CheckedListBox fflagConfig;
        private System.Windows.Forms.ListBox availableFFlags;
        private System.Windows.Forms.Button add;
        private System.Windows.Forms.Button remove;
        private System.Windows.Forms.Label availableTitle;
        private System.Windows.Forms.Label configTitle;
        private System.Windows.Forms.TextBox searchFilter;
        private System.Windows.Forms.Label searchFilterLbl;
    }
}