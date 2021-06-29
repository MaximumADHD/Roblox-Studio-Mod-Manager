
namespace RobloxStudioModManager
{
    partial class LayoutManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutManager));
            this.saveCurrentLayout = new System.Windows.Forms.Button();
            this.presetList = new System.Windows.Forms.ListBox();
            this.loadSelectedLayout = new System.Windows.Forms.Button();
            this.deleteSelectedLayout = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // saveCurrentLayout
            // 
            this.saveCurrentLayout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveCurrentLayout.Location = new System.Drawing.Point(138, 12);
            this.saveCurrentLayout.Name = "saveCurrentLayout";
            this.saveCurrentLayout.Size = new System.Drawing.Size(124, 23);
            this.saveCurrentLayout.TabIndex = 0;
            this.saveCurrentLayout.Text = "Save Layout";
            this.saveCurrentLayout.UseVisualStyleBackColor = true;
            this.saveCurrentLayout.Click += new System.EventHandler(this.saveCurrentLayout_Click);
            // 
            // presetList
            // 
            this.presetList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.presetList.FormattingEnabled = true;
            this.presetList.ItemHeight = 15;
            this.presetList.Location = new System.Drawing.Point(12, 12);
            this.presetList.Name = "presetList";
            this.presetList.Size = new System.Drawing.Size(120, 154);
            this.presetList.TabIndex = 1;
            this.presetList.SelectedIndexChanged += new System.EventHandler(this.presetList_SelectedIndexChanged);
            this.presetList.DoubleClick += new System.EventHandler(this.presetList_DoubleClick);
            // 
            // loadSelectedLayout
            // 
            this.loadSelectedLayout.Enabled = false;
            this.loadSelectedLayout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadSelectedLayout.Location = new System.Drawing.Point(138, 41);
            this.loadSelectedLayout.Name = "loadSelectedLayout";
            this.loadSelectedLayout.Size = new System.Drawing.Size(124, 23);
            this.loadSelectedLayout.TabIndex = 2;
            this.loadSelectedLayout.Text = "Load Preset";
            this.loadSelectedLayout.UseVisualStyleBackColor = true;
            this.loadSelectedLayout.Click += new System.EventHandler(this.loadSelectedLayout_Click);
            // 
            // deleteSelectedLayout
            // 
            this.deleteSelectedLayout.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deleteSelectedLayout.Location = new System.Drawing.Point(138, 70);
            this.deleteSelectedLayout.Name = "deleteSelectedLayout";
            this.deleteSelectedLayout.Size = new System.Drawing.Size(123, 23);
            this.deleteSelectedLayout.TabIndex = 3;
            this.deleteSelectedLayout.Text = "Delete Preset";
            this.deleteSelectedLayout.UseVisualStyleBackColor = true;
            this.deleteSelectedLayout.Click += new System.EventHandler(this.deleteSelectedLayout_Click);
            // 
            // LayoutManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(273, 183);
            this.Controls.Add(this.deleteSelectedLayout);
            this.Controls.Add(this.loadSelectedLayout);
            this.Controls.Add(this.presetList);
            this.Controls.Add(this.saveCurrentLayout);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LayoutManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Layout Manager";
            this.Load += new System.EventHandler(this.LayoutManager_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button saveCurrentLayout;
        private System.Windows.Forms.ListBox presetList;
        private System.Windows.Forms.Button loadSelectedLayout;
        private System.Windows.Forms.Button deleteSelectedLayout;
    }
}