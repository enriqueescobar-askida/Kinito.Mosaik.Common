namespace ReviewCode
{
    partial class testFormCredential
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
            this.textBoxCredentialPassword = new System.Windows.Forms.TextBox();
            this.buttonCredential = new System.Windows.Forms.Button();
            this.labelCredential = new System.Windows.Forms.Label();
            this.labelPasswordRequire = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxCredentialPassword
            // 
            this.textBoxCredentialPassword.Location = new System.Drawing.Point(32, 77);
            this.textBoxCredentialPassword.Name = "textBoxCredentialPassword";
            this.textBoxCredentialPassword.Size = new System.Drawing.Size(100, 20);
            this.textBoxCredentialPassword.TabIndex = 0;
            this.textBoxCredentialPassword.UseSystemPasswordChar = true;
            // 
            // buttonCredential
            // 
            this.buttonCredential.Location = new System.Drawing.Point(151, 45);
            this.buttonCredential.Name = "buttonCredential";
            this.buttonCredential.Size = new System.Drawing.Size(99, 29);
            this.buttonCredential.TabIndex = 1;
            this.buttonCredential.Text = "Sign-in";
            this.buttonCredential.UseVisualStyleBackColor = true;
            this.buttonCredential.Click += new System.EventHandler(this.buttonCredential_Click);
            // 
            // labelCredential
            // 
            this.labelCredential.AutoSize = true;
            this.labelCredential.Location = new System.Drawing.Point(10, 61);
            this.labelCredential.Name = "labelCredential";
            this.labelCredential.Size = new System.Drawing.Size(122, 13);
            this.labelCredential.TabIndex = 2;
            this.labelCredential.Text = "Please Enter Password :";
            // 
            // labelPasswordRequire
            // 
            this.labelPasswordRequire.AutoSize = true;
            this.labelPasswordRequire.Location = new System.Drawing.Point(29, 100);
            this.labelPasswordRequire.Name = "labelPasswordRequire";
            this.labelPasswordRequire.Size = new System.Drawing.Size(0, 13);
            this.labelPasswordRequire.TabIndex = 3;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(32, 23);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(100, 20);
            this.textBoxUsername.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Please Enter Username (e.g: coXX) :";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(152, 84);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(98, 29);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // testFormCredential
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 130);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.labelPasswordRequire);
            this.Controls.Add(this.labelCredential);
            this.Controls.Add(this.buttonCredential);
            this.Controls.Add(this.textBoxCredentialPassword);
            this.Name = "testFormCredential";
            this.Text = "TFS Connection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxCredentialPassword;
        private System.Windows.Forms.Button buttonCredential;
        private System.Windows.Forms.Label labelCredential;
        private System.Windows.Forms.Label labelPasswordRequire;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
    }
}