namespace GenerateLicenseApplication
{
    partial class LicenseGenerator
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
            this.labelClientKey = new System.Windows.Forms.Label();
            this.textBoxClientKey = new System.Windows.Forms.TextBox();
            this.buttonImportFromFile = new System.Windows.Forms.Button();
            this.groupBoxConfiguration = new System.Windows.Forms.GroupBox();
            this.checkBoxUseDateRange = new System.Windows.Forms.CheckBox();
            this.dateTimePickerEndDate = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerStartDate = new System.Windows.Forms.DateTimePicker();
            this.comboBoxLicenseType = new System.Windows.Forms.ComboBox();
            this.labelEndDate = new System.Windows.Forms.Label();
            this.labelStartDate = new System.Windows.Forms.Label();
            this.labelLicenseType = new System.Windows.Forms.Label();
            this.buttonGenerateLicense = new System.Windows.Forms.Button();
            this.ClientKeyDecrypted = new System.Windows.Forms.Label();
            this.loadLicenceButton = new System.Windows.Forms.Button();
            this.groupBoxConfiguration.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelClientKey
            // 
            this.labelClientKey.AutoSize = true;
            this.labelClientKey.Location = new System.Drawing.Point(13, 13);
            this.labelClientKey.Name = "labelClientKey";
            this.labelClientKey.Size = new System.Drawing.Size(64, 13);
            this.labelClientKey.TabIndex = 0;
            this.labelClientKey.Text = "Ckient Key :";
            // 
            // textBoxClientKey
            // 
            this.textBoxClientKey.Location = new System.Drawing.Point(84, 13);
            this.textBoxClientKey.Name = "textBoxClientKey";
            this.textBoxClientKey.Size = new System.Drawing.Size(667, 20);
            this.textBoxClientKey.TabIndex = 1;
            this.textBoxClientKey.TextChanged += new System.EventHandler(this.TextBoxClientKeyTextChanged);
            this.textBoxClientKey.Validating += new System.ComponentModel.CancelEventHandler(this.OnTextBoxClientKeyValidating);
            // 
            // buttonImportFromFile
            // 
            this.buttonImportFromFile.Location = new System.Drawing.Point(758, 13);
            this.buttonImportFromFile.Name = "buttonImportFromFile";
            this.buttonImportFromFile.Size = new System.Drawing.Size(111, 23);
            this.buttonImportFromFile.TabIndex = 2;
            this.buttonImportFromFile.Text = "Import From File";
            this.buttonImportFromFile.UseVisualStyleBackColor = true;
            this.buttonImportFromFile.Click += new System.EventHandler(this.OnButtonImportFromFileClick);
            // 
            // groupBoxConfiguration
            // 
            this.groupBoxConfiguration.Controls.Add(this.checkBoxUseDateRange);
            this.groupBoxConfiguration.Controls.Add(this.dateTimePickerEndDate);
            this.groupBoxConfiguration.Controls.Add(this.dateTimePickerStartDate);
            this.groupBoxConfiguration.Controls.Add(this.comboBoxLicenseType);
            this.groupBoxConfiguration.Controls.Add(this.labelEndDate);
            this.groupBoxConfiguration.Controls.Add(this.labelStartDate);
            this.groupBoxConfiguration.Controls.Add(this.labelLicenseType);
            this.groupBoxConfiguration.Location = new System.Drawing.Point(265, 93);
            this.groupBoxConfiguration.Name = "groupBoxConfiguration";
            this.groupBoxConfiguration.Size = new System.Drawing.Size(338, 146);
            this.groupBoxConfiguration.TabIndex = 3;
            this.groupBoxConfiguration.TabStop = false;
            this.groupBoxConfiguration.Text = "Configuration";
            // 
            // checkBoxUseDateRange
            // 
            this.checkBoxUseDateRange.AutoSize = true;
            this.checkBoxUseDateRange.Location = new System.Drawing.Point(10, 114);
            this.checkBoxUseDateRange.Name = "checkBoxUseDateRange";
            this.checkBoxUseDateRange.Size = new System.Drawing.Size(106, 17);
            this.checkBoxUseDateRange.TabIndex = 6;
            this.checkBoxUseDateRange.Text = "Use Date Range";
            this.checkBoxUseDateRange.UseVisualStyleBackColor = true;
            this.checkBoxUseDateRange.Visible = false;
            // 
            // dateTimePickerEndDate
            // 
            this.dateTimePickerEndDate.Location = new System.Drawing.Point(91, 85);
            this.dateTimePickerEndDate.Name = "dateTimePickerEndDate";
            this.dateTimePickerEndDate.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerEndDate.TabIndex = 5;
            // 
            // dateTimePickerStartDate
            // 
            this.dateTimePickerStartDate.Location = new System.Drawing.Point(91, 58);
            this.dateTimePickerStartDate.Name = "dateTimePickerStartDate";
            this.dateTimePickerStartDate.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerStartDate.TabIndex = 4;
            // 
            // comboBoxLicenseType
            // 
            this.comboBoxLicenseType.FormattingEnabled = true;
            this.comboBoxLicenseType.Location = new System.Drawing.Point(91, 30);
            this.comboBoxLicenseType.Name = "comboBoxLicenseType";
            this.comboBoxLicenseType.Size = new System.Drawing.Size(200, 21);
            this.comboBoxLicenseType.TabIndex = 3;
            this.comboBoxLicenseType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxLicenseTypeSelectedIndexChanged);
            // 
            // labelEndDate
            // 
            this.labelEndDate.AutoSize = true;
            this.labelEndDate.Location = new System.Drawing.Point(7, 88);
            this.labelEndDate.Name = "labelEndDate";
            this.labelEndDate.Size = new System.Drawing.Size(58, 13);
            this.labelEndDate.TabIndex = 2;
            this.labelEndDate.Text = "End Date :";
            // 
            // labelStartDate
            // 
            this.labelStartDate.AutoSize = true;
            this.labelStartDate.Location = new System.Drawing.Point(7, 62);
            this.labelStartDate.Name = "labelStartDate";
            this.labelStartDate.Size = new System.Drawing.Size(61, 13);
            this.labelStartDate.TabIndex = 1;
            this.labelStartDate.Text = "Start Date :";
            // 
            // labelLicenseType
            // 
            this.labelLicenseType.AutoSize = true;
            this.labelLicenseType.Location = new System.Drawing.Point(7, 30);
            this.labelLicenseType.Name = "labelLicenseType";
            this.labelLicenseType.Size = new System.Drawing.Size(77, 13);
            this.labelLicenseType.TabIndex = 0;
            this.labelLicenseType.Text = "License Type :";
            // 
            // buttonGenerateLicense
            // 
            this.buttonGenerateLicense.Location = new System.Drawing.Point(377, 353);
            this.buttonGenerateLicense.Name = "buttonGenerateLicense";
            this.buttonGenerateLicense.Size = new System.Drawing.Size(133, 23);
            this.buttonGenerateLicense.TabIndex = 4;
            this.buttonGenerateLicense.Text = "Generate License";
            this.buttonGenerateLicense.UseVisualStyleBackColor = true;
            this.buttonGenerateLicense.Click += new System.EventHandler(this.OnButtonGenerateLicenseClick);
            // 
            // ClientKeyDecrypted
            // 
            this.ClientKeyDecrypted.AutoSize = true;
            this.ClientKeyDecrypted.Location = new System.Drawing.Point(84, 49);
            this.ClientKeyDecrypted.Name = "ClientKeyDecrypted";
            this.ClientKeyDecrypted.Size = new System.Drawing.Size(0, 13);
            this.ClientKeyDecrypted.TabIndex = 5;
            // 
            // loadLicenceButton
            // 
            this.loadLicenceButton.Location = new System.Drawing.Point(517, 353);
            this.loadLicenceButton.Name = "loadLicenceButton";
            this.loadLicenceButton.Size = new System.Drawing.Size(111, 23);
            this.loadLicenceButton.TabIndex = 6;
            this.loadLicenceButton.Text = "LoadLicence";
            this.loadLicenceButton.UseVisualStyleBackColor = true;
            this.loadLicenceButton.Click += new System.EventHandler(this.LoadLicenceButtonClick);
            // 
            // LicenseGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.ClientSize = new System.Drawing.Size(891, 398);
            this.Controls.Add(this.loadLicenceButton);
            this.Controls.Add(this.ClientKeyDecrypted);
            this.Controls.Add(this.buttonGenerateLicense);
            this.Controls.Add(this.groupBoxConfiguration);
            this.Controls.Add(this.buttonImportFromFile);
            this.Controls.Add(this.textBoxClientKey);
            this.Controls.Add(this.labelClientKey);
            this.Name = "LicenseGenerator";
            this.Text = "License Generator";
            this.Load += new System.EventHandler(this.OnLoad);
            this.groupBoxConfiguration.ResumeLayout(false);
            this.groupBoxConfiguration.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelClientKey;
        private System.Windows.Forms.TextBox textBoxClientKey;
        private System.Windows.Forms.Button buttonImportFromFile;
        private System.Windows.Forms.GroupBox groupBoxConfiguration;
        private System.Windows.Forms.DateTimePicker dateTimePickerEndDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartDate;
        private System.Windows.Forms.ComboBox comboBoxLicenseType;
        private System.Windows.Forms.Label labelEndDate;
        private System.Windows.Forms.Label labelStartDate;
        private System.Windows.Forms.Label labelLicenseType;
        private System.Windows.Forms.Button buttonGenerateLicense;
        private System.Windows.Forms.CheckBox checkBoxUseDateRange;
        private System.Windows.Forms.Label ClientKeyDecrypted;
        private System.Windows.Forms.Button loadLicenceButton;
    }
}

