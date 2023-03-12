namespace LabTests__5CSRTT
{
    partial class CreateNewAnalysisForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.label__ProtocolId = new System.Windows.Forms.Label();
            this.ProtocolIdComboBox = new System.Windows.Forms.ComboBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.CreateButton = new System.Windows.Forms.Button();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(12, 27);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(300, 23);
            this.NameTextBox.TabIndex = 1;
            // 
            // label__ProtocolId
            // 
            this.label__ProtocolId.AutoSize = true;
            this.label__ProtocolId.Location = new System.Drawing.Point(12, 62);
            this.label__ProtocolId.Name = "label__ProtocolId";
            this.label__ProtocolId.Size = new System.Drawing.Size(52, 15);
            this.label__ProtocolId.TabIndex = 2;
            this.label__ProtocolId.Text = "Protocol";
            // 
            // ProtocolIdComboBox
            // 
            this.ProtocolIdComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProtocolIdComboBox.FormattingEnabled = true;
            this.ProtocolIdComboBox.Location = new System.Drawing.Point(12, 80);
            this.ProtocolIdComboBox.Name = "ProtocolIdComboBox";
            this.ProtocolIdComboBox.Size = new System.Drawing.Size(300, 23);
            this.ProtocolIdComboBox.TabIndex = 3;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(129, 117);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 33);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // CreateButton
            // 
            this.CreateButton.Image = global::LabTests__5CSRTT.Properties.Resources.application_form_add;
            this.CreateButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CreateButton.Location = new System.Drawing.Point(210, 117);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.CreateButton.Size = new System.Drawing.Size(102, 33);
            this.CreateButton.TabIndex = 5;
            this.CreateButton.Text = "Create";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = true;
            this.ErrorLabel.ForeColor = System.Drawing.Color.IndianRed;
            this.ErrorLabel.Location = new System.Drawing.Point(13, 117);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Size = new System.Drawing.Size(0, 15);
            this.ErrorLabel.TabIndex = 6;
            // 
            // CreateNewAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 161);
            this.Controls.Add(this.ErrorLabel);
            this.Controls.Add(this.CreateButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.ProtocolIdComboBox);
            this.Controls.Add(this.label__ProtocolId);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label1);
            this.Name = "CreateNewAnalysisForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "New";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private TextBox NameTextBox;
        private Label label__ProtocolId;
        private ComboBox ProtocolIdComboBox;
        private Button CancelButton;
        private Button CreateButton;
        private Label ErrorLabel;
    }
}