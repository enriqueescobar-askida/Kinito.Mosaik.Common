namespace AlphaMosaik.Logger.Debugging.ServiceEmulator
{
    partial class Form1
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
            this.txBoxMessage = new System.Windows.Forms.TextBox();
            this.btAddEntry = new System.Windows.Forms.Button();
            this.btEnableLogger = new System.Windows.Forms.Button();
            this.btDisableLogger = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txBoxMessage
            // 
            this.txBoxMessage.Location = new System.Drawing.Point(13, 13);
            this.txBoxMessage.Name = "txBoxMessage";
            this.txBoxMessage.Size = new System.Drawing.Size(178, 20);
            this.txBoxMessage.TabIndex = 0;
            // 
            // btAddEntry
            // 
            this.btAddEntry.Location = new System.Drawing.Point(197, 11);
            this.btAddEntry.Name = "btAddEntry";
            this.btAddEntry.Size = new System.Drawing.Size(75, 23);
            this.btAddEntry.TabIndex = 1;
            this.btAddEntry.Text = "Log entry";
            this.btAddEntry.UseVisualStyleBackColor = true;
            this.btAddEntry.Click += new System.EventHandler(this.btAddEntry_Click);
            // 
            // btEnableLogger
            // 
            this.btEnableLogger.Location = new System.Drawing.Point(13, 40);
            this.btEnableLogger.Name = "btEnableLogger";
            this.btEnableLogger.Size = new System.Drawing.Size(75, 23);
            this.btEnableLogger.TabIndex = 2;
            this.btEnableLogger.Text = "Activate";
            this.btEnableLogger.UseVisualStyleBackColor = true;
            this.btEnableLogger.Click += new System.EventHandler(this.btEnableLogger_Click);
            // 
            // btDisableLogger
            // 
            this.btDisableLogger.Location = new System.Drawing.Point(94, 40);
            this.btDisableLogger.Name = "btDisableLogger";
            this.btDisableLogger.Size = new System.Drawing.Size(75, 23);
            this.btDisableLogger.TabIndex = 3;
            this.btDisableLogger.Text = "Deactivate";
            this.btDisableLogger.UseVisualStyleBackColor = true;
            this.btDisableLogger.Click += new System.EventHandler(this.btDisableLogger_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 76);
            this.Controls.Add(this.btDisableLogger);
            this.Controls.Add(this.btEnableLogger);
            this.Controls.Add(this.btAddEntry);
            this.Controls.Add(this.txBoxMessage);
            this.Name = "Form1";
            this.Text = "AlphaMosaik Logger Service Emulator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txBoxMessage;
        private System.Windows.Forms.Button btAddEntry;
        private System.Windows.Forms.Button btEnableLogger;
        private System.Windows.Forms.Button btDisableLogger;
    }
}

