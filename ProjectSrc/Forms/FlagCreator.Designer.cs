
namespace RobloxStudioModManager
{
    partial class FlagCreator
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
            this.nameLabel = new System.Windows.Forms.Label();
            this.flagName = new System.Windows.Forms.TextBox();
            this.typeLabel = new System.Windows.Forms.Label();
            this.flagType = new System.Windows.Forms.ComboBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.flagClass = new System.Windows.Forms.ComboBox();
            this.classLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(12, 8);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(63, 25);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // flagName
            // 
            this.flagName.Location = new System.Drawing.Point(81, 11);
            this.flagName.Name = "flagName";
            this.flagName.Size = new System.Drawing.Size(235, 26);
            this.flagName.TabIndex = 1;
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.typeLabel.Location = new System.Drawing.Point(12, 81);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(53, 25);
            this.typeLabel.TabIndex = 2;
            this.typeLabel.Text = "Type:";
            // 
            // flagType
            // 
            this.flagType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.flagType.FormattingEnabled = true;
            this.flagType.Items.AddRange(new object[] {
            "Flag",
            "Int",
            "Log",
            "String"});
            this.flagType.Location = new System.Drawing.Point(81, 84);
            this.flagType.Name = "flagType";
            this.flagType.Size = new System.Drawing.Size(235, 28);
            this.flagType.TabIndex = 3;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(17, 130);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(142, 33);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // createButton
            // 
            this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location = new System.Drawing.Point(174, 130);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(142, 33);
            this.createButton.TabIndex = 5;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // flagClass
            // 
            this.flagClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.flagClass.FormattingEnabled = true;
            this.flagClass.Items.AddRange(new object[] {
            "Fast (F)",
            "Dynamic Fast (DF)",
            "Synchronized Fast (SF)"});
            this.flagClass.Location = new System.Drawing.Point(81, 47);
            this.flagClass.Name = "flagClass";
            this.flagClass.Size = new System.Drawing.Size(235, 28);
            this.flagClass.TabIndex = 7;
            // 
            // classLabel
            // 
            this.classLabel.AutoSize = true;
            this.classLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.classLabel.Location = new System.Drawing.Point(12, 45);
            this.classLabel.Name = "classLabel";
            this.classLabel.Size = new System.Drawing.Size(56, 25);
            this.classLabel.TabIndex = 6;
            this.classLabel.Text = "Class:";
            // 
            // FlagCreator
            // 
            this.AcceptButton = this.createButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(331, 176);
            this.Controls.Add(this.flagClass);
            this.Controls.Add(this.classLabel);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.flagType);
            this.Controls.Add(this.typeLabel);
            this.Controls.Add(this.flagName);
            this.Controls.Add(this.nameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FlagCreator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Custom Flag";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox flagName;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.ComboBox flagType;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.ComboBox flagClass;
        private System.Windows.Forms.Label classLabel;
    }
}