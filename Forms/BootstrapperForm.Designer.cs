﻿namespace RobloxStudioModManager
{
    partial class BootstrapperForm
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.logo = new System.Windows.Forms.PictureBox();
            this.statusLbl = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.RichTextBox();
            this.progressMetric = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.logo)).BeginInit();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(9, 291);
            this.progressBar.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar.MarqueeAnimationSpeed = 1;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(873, 44);
            this.progressBar.Step = 0;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            this.progressBar.Value = 100;
            // 
            // logo
            // 
            this.logo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.logo.BackColor = System.Drawing.Color.Transparent;
            this.logo.BackgroundImage = global::RobloxStudioModManager.Properties.Resources.Logo;
            this.logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.logo.InitialImage = global::RobloxStudioModManager.Properties.Resources.Logo;
            this.logo.Location = new System.Drawing.Point(9, 12);
            this.logo.Name = "logo";
            this.logo.Size = new System.Drawing.Size(182, 229);
            this.logo.TabIndex = 9;
            this.logo.TabStop = false;
            this.logo.WaitOnLoad = true;
            // 
            // statusLbl
            // 
            this.statusLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLbl.AutoSize = true;
            this.statusLbl.Font = new System.Drawing.Font("Segoe UI Light", 16F);
            this.statusLbl.Location = new System.Drawing.Point(0, 244);
            this.statusLbl.Margin = new System.Windows.Forms.Padding(0);
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(327, 45);
            this.statusLbl.TabIndex = 11;
            this.statusLbl.Text = "Checking for updates...";
            this.statusLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // log
            // 
            this.log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.log.BackColor = System.Drawing.Color.White;
            this.log.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.log.ForeColor = System.Drawing.Color.Black;
            this.log.Location = new System.Drawing.Point(197, 12);
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.Size = new System.Drawing.Size(685, 229);
            this.log.TabIndex = 12;
            this.log.Text = "";
            // 
            // progressMetric
            // 
            this.progressMetric.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressMetric.AutoSize = true;
            this.progressMetric.BackColor = System.Drawing.Color.Transparent;
            this.progressMetric.Font = new System.Drawing.Font("Segoe UI Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressMetric.Location = new System.Drawing.Point(4, 339);
            this.progressMetric.Name = "progressMetric";
            this.progressMetric.Size = new System.Drawing.Size(52, 25);
            this.progressMetric.TabIndex = 13;
            this.progressMetric.Text = "0/100";
            // 
            // BootstrapperForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(894, 373);
            this.Controls.Add(this.progressMetric);
            this.Controls.Add(this.log);
            this.Controls.Add(this.statusLbl);
            this.Controls.Add(this.logo);
            this.Controls.Add(this.progressBar);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::RobloxStudioModManager.Properties.Resources.Icon;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(668, 288);
            this.Name = "BootstrapperForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Roblox Studio Bootstrapper";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BootstrapperForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BootstrapperForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.logo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox logo;
        private System.Windows.Forms.Label statusLbl;
        private System.Windows.Forms.RichTextBox log;
        private System.Windows.Forms.Label progressMetric;
    }
}