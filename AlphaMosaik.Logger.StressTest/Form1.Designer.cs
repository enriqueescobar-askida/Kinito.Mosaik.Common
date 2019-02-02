namespace AlphaMosaik.Logger.StressTest
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
            this.button1 = new System.Windows.Forms.Button();
            this.TextBoxSpeed = new System.Windows.Forms.TextBox();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.labelMax = new System.Windows.Forms.Label();
            this.TextBoxMax = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.StressWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(248, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Stress";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TextBoxSpeed
            // 
            this.TextBoxSpeed.Location = new System.Drawing.Point(112, 48);
            this.TextBoxSpeed.Name = "TextBoxSpeed";
            this.TextBoxSpeed.Size = new System.Drawing.Size(149, 20);
            this.TextBoxSpeed.TabIndex = 1;
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.labelSpeed.Location = new System.Drawing.Point(12, 51);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(66, 13);
            this.labelSpeed.TabIndex = 2;
            this.labelSpeed.Text = "Speed (ms) :";
            // 
            // labelMax
            // 
            this.labelMax.AutoSize = true;
            this.labelMax.Location = new System.Drawing.Point(10, 71);
            this.labelMax.Name = "labelMax";
            this.labelMax.Size = new System.Drawing.Size(96, 13);
            this.labelMax.TabIndex = 3;
            this.labelMax.Text = "Number of entries :";
            // 
            // TextBoxMax
            // 
            this.TextBoxMax.Location = new System.Drawing.Point(112, 68);
            this.TextBoxMax.Name = "TextBoxMax";
            this.TextBoxMax.Size = new System.Drawing.Size(149, 20);
            this.TextBoxMax.TabIndex = 4;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 95);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(255, 23);
            this.progressBar.TabIndex = 5;
            // 
            // StressWorker
            // 
            this.StressWorker.WorkerReportsProgress = true;
            this.StressWorker.WorkerSupportsCancellation = true;
            this.StressWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.StressWorker_DoWork);
            this.StressWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.StressWorker_RunWorkerCompleted);
            this.StressWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.StressWorker_ProgressChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 130);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.TextBoxMax);
            this.Controls.Add(this.labelMax);
            this.Controls.Add(this.labelSpeed);
            this.Controls.Add(this.TextBoxSpeed);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Stress Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox TextBoxSpeed;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.Label labelMax;
        private System.Windows.Forms.TextBox TextBoxMax;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.ComponentModel.BackgroundWorker StressWorker;
    }
}

